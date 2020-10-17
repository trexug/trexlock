using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TrexLock.Gpio;
using TrexLock.Persistence;
using TrexLock.Persistence.Dto;
using Unosquare.RaspberryIO.Abstractions;

namespace TrexLock.Locking
{
	public class LockManager
	{
		private LockDbContext LockDbContext { get; }
		private Dictionary<string, Lock> IdToLock { get; }
		private ILogger<LockManager> Logger { get; }
		private Timer Timer { get; }
		private SemaphoreSlim DbSemaphore { get; }
		private SemaphoreSlim TimerSemaphore { get; }

		public LockManager(ILoggerFactory loggerFactory, IGpioPinFactory gpioPinFactory, IOptions<LockOptions> lockOptions, LockDbContext lockDbContext)
		{
			IdToLock = new Dictionary<string, Lock>();
			Logger = loggerFactory.CreateLogger<LockManager>();
			LockDbContext = lockDbContext;
			Timer = new Timer(TimerExpired);
			DbSemaphore = new SemaphoreSlim(1);
			TimerSemaphore = new SemaphoreSlim(1);
			Initialize(lockOptions.Value, gpioPinFactory, lockDbContext);
		}

		private void Initialize(LockOptions lockOptions, IGpioPinFactory gpioPinFactory, LockDbContext lockDbContext)
		{
			async Task InitializeLock(LockConfiguration lockConfiguration)
			{
				LockDto lockDto = await lockDbContext.Locks.FindAsync(lockConfiguration.Id);
				LockState mode = lockDto?.State ?? LockState.Locked;
				Lock @lock = new Lock(lockConfiguration.Id, gpioPinFactory.CreatePin((BcmPin)lockConfiguration.Pin), (LockState)(-1));
				@lock.Timeout = lockDto?.Timeout;
				await SetAsync(@lock, mode, "INITIALIZE");
				IdToLock.Add(@lock.Id, @lock);
			}

			Task.WaitAll(lockOptions.LockConfigurations.Select(InitializeLock).ToArray());
			UpdateTimerAsync().Wait();
		}

		public IEnumerable<string> Locks => IdToLock.Keys;

		public async Task ToggleLockAsync(string id, string reason)
		{
			if (!IdToLock.TryGetValue(id, out Lock @lock))
			{
				throw new ArgumentException("Unknown id", nameof(id));
			}

			await @lock.Semaphore.WaitAsync();
			try
			{
				@lock.Timeout = null;
				LockState newState = await ToggleAsync(@lock, reason);
				await DbSemaphore.WaitAsync();
				try
				{
					await UpdateLockDto(id, newState, null);
				}
				finally
				{
					DbSemaphore.Release();
				}
				
			}
			finally
			{
				@lock.Semaphore.Release();
			}
			await UpdateTimerAsync();
		}

		public async Task SetLockAsync(string id, LockState state, string reason, DateTime? timeout = null)
		{
			if (!IdToLock.TryGetValue(id, out Lock @lock))
			{
				throw new ArgumentException("Unknown id", nameof(id));
			}

			await @lock.Semaphore.WaitAsync();
			try
			{
				@lock.Timeout = timeout;
				await SetAsync(@lock, state, reason);
				await DbSemaphore.WaitAsync();
				try
				{
					await UpdateLockDto(id, state, timeout);
				}
				finally
				{
					DbSemaphore.Release();
				}
			}
			finally
			{
				@lock.Semaphore.Release();
			}
			await UpdateTimerAsync();
		}

		private async Task UpdateLockDto(string id, LockState state, DateTime? timeout)
		{
			LockDto lockDto = LockDbContext.Locks.Find(id);
			if (lockDto == null)
			{
				lockDto = new LockDto()
				{
					Id = id,
					State = state,
					Timeout = timeout
				};
				LockDbContext.Add(lockDto);
			}
			else
			{
				lockDto.State = state;
				lockDto.Timeout = timeout;
				LockDbContext.Update(lockDto);
			}
			await LockDbContext.SaveChangesAsync();
		}

		private void TimerExpired(object obj)
		{
			TimerExpiredAsync(obj).Wait();
		}

		private async Task TimerExpiredAsync(object obj)
		{
			async Task WaitToggleRelease(Lock @lock)
			{
				await @lock.Semaphore.WaitAsync();
				@lock.Timeout = null;
				await ToggleAsync(@lock, "TIMEOUT");
				@lock.Semaphore.Release();
			}

			DateTime now = DateTime.Now;
			var locks = IdToLock.Values
				.Where(l => l.Timeout <= now).ToList();
			var waitLocksTasks = locks.Select(WaitToggleRelease);
			await Task.WhenAll(waitLocksTasks);
			await UpdateTimerAsync();
		}

		private async Task UpdateTimerAsync()
		{
			await TimerSemaphore.WaitAsync();
			try
			{
				var waitLocksTask = IdToLock.Values.Select(l => l.Semaphore.WaitAsync());
				DateTime? next;
				await Task.WhenAll(waitLocksTask);
				try
				{
					next = IdToLock.Values
						.Where(l => l.Timeout.HasValue)
						.OrderBy(l => l.Timeout.Value)
						.FirstOrDefault()?.Timeout;
				}
				finally
				{
					foreach (Lock @lock in IdToLock.Values)
					{
						@lock.Semaphore.Release();
					}
				}

				int timeout;
				if (next.HasValue)
				{
					DateTime now = DateTime.Now;
					timeout = Math.Max((int)(next.Value - now).TotalMilliseconds, 0);
				}
				else
				{
					timeout = -1;
				}
				Timer.Change(timeout, -1);
			}
			finally
			{
				TimerSemaphore.Release();
			}
		}

		private async Task<LockState> ToggleAsync(Lock @lock, string reason)
		{
			DateTime now = DateTime.Now;
			LockState targetState = @lock.State.Toggle();
			@lock.State = targetState;
			PinLogDto pinLog = new PinLogDto()
			{
				Pin = @lock.PinId,
				PinState = Lock.ToPinState(@lock.State),
				Reason = reason,
				Time = now
			};
			Logger.LogInformation("{0} toggled to {1} ({2})", @lock.Id, targetState, reason);
			await DbSemaphore.WaitAsync();
			try
			{
				LockDbContext.Add(pinLog);
				await LockDbContext.SaveChangesAsync();
			}
			finally
			{
				DbSemaphore.Release();
			}
			return targetState;
		}

		private async Task SetAsync(Lock @lock, LockState mode, string reason)
		{
			if (@lock.State != mode)
			{
				DateTime now = DateTime.Now;
				@lock.State = mode;
				PinLogDto pinLog = new PinLogDto()
				{
					Pin = @lock.PinId,
					PinState = Lock.ToPinState(@lock.State),
					Reason = reason,
					Time = now
				};
				Logger.LogInformation("{0} set to {1} ({2})", @lock.Id, mode, reason);
				await DbSemaphore.WaitAsync();
				try
				{
					LockDbContext.Add(pinLog);
					await LockDbContext.SaveChangesAsync();
				}
				finally
				{
					DbSemaphore.Release();
				}
			}
			else
			{
				Logger.LogInformation("{0} is already in mode {1} ({2})", @lock.Id, mode, reason);
			}
		}
	}
}
