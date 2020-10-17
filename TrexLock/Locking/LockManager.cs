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

		public LockManager(ILoggerFactory loggerFactory, IGpioPinFactory gpioPinFactory, IOptions<LockOptions> lockOptions, LockDbContext lockDbContext)
		{
			IdToLock = new Dictionary<string, Lock>();
			Logger = loggerFactory.CreateLogger<LockManager>();
			LockDbContext = lockDbContext;
			Timer = new Timer(TimerExpired);

			Initialize(lockOptions.Value, gpioPinFactory, lockDbContext).Wait();
		}

		private async Task Initialize(LockOptions lockOptions, IGpioPinFactory gpioPinFactory, LockDbContext lockDbContext)
		{
			foreach (LockConfiguration lockConfiguration in lockOptions.LockConfigurations)
			{
				LockState state = lockDbContext.Locks.Find(lockConfiguration.Id);
				LockMode mode = state?.Mode ?? LockMode.Lock;
				Lock @lock = new Lock(lockConfiguration.Id, gpioPinFactory.CreatePin((BcmPin)lockConfiguration.Pin), (LockMode)(-1));
				await SetAsync(@lock, mode, "INITIALIZE");
				IdToLock.Add(@lock.Id, @lock);
			}
		}

		public IEnumerable<string> Locks => IdToLock.Keys;

		public Task SetLockAsync(string id, LockMode mode, string reason, DateTime? timeout = null)
		{
			if (!IdToLock.TryGetValue(id, out Lock @lock))
			{
				throw new ArgumentException("Unknown id", nameof(id));
			}

			lock (this)
			{
				SetAsync(@lock, mode, reason).Wait();
				@lock.Timeout = timeout.Value;
				UpdateTimer();

				LockState lockState = LockDbContext.Locks.Find(id);
				if (lockState == null)
				{
					lockState = new LockState()
					{
						Id = id,
						Mode = mode,
						Timeout = timeout
					};
					LockDbContext.Add(lockState);
				}
				else
				{
					lockState.Mode = mode;
					lockState.Timeout = timeout;
					LockDbContext.Update(lockState);
				}
			}
			return LockDbContext.SaveChangesAsync();
		}

		private void TimerExpired(object obj)
		{
			DateTime now = DateTime.Now;
			foreach (Lock @lock in IdToLock.Values.Where(l => l.Timeout <= now))
			{
				@lock.Timeout = null;
				ToggleLockAsync(@lock, "TIMEOUT").Wait();
			}
			UpdateTimer();
		}

		private void UpdateTimer()
		{
			DateTime? next = IdToLock.Values
				.Where(l => l.Timeout.HasValue)
				.OrderBy(l => l.Timeout.Value)
				.FirstOrDefault()?.Timeout;
			
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

		private Task SetAsync(Lock @lock, LockMode mode, string reason)
		{
			if (@lock.Status != mode)
			{
				lock (this)
				{
					DateTime now = DateTime.Now;
					@lock.Status = mode;
					PinLog pinLog = new PinLog()
					{
						Pin = @lock.PinId,
						PinState = Lock.ToPinState(@lock.Status),
						Reason = reason,
						Time = now
					};
					Logger.LogInformation("{0} set to {1} ({2})", @lock.Id, mode, reason);
					LockDbContext.Add(pinLog);
				}
				return LockDbContext.SaveChangesAsync();
			}
			else
			{
				Logger.LogInformation("{0} is already in mode {1} ({2})", @lock.Id, mode, reason);
			}
			return Task.CompletedTask;
		}

		public Task ToggleLockAsync(Lock @lock, string reason)
		{
			DateTime now = DateTime.Now;
			lock (this)
			{
				@lock.Toggle();
				PinLog pinLog = new PinLog()
				{
					Pin = @lock.PinId,
					PinState = Lock.ToPinState(@lock.Status),
					Reason = reason,
					Time = now
				};
				Logger.LogInformation("{0} was toggled to {1} ({2})", @lock.Id, @lock.Status, reason);
				LockDbContext.Add(pinLog);
			}
			return LockDbContext.SaveChangesAsync();
		}
	}
}
