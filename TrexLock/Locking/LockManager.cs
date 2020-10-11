using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
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
		private Dictionary<string, Lock> IdToPin { get; }
		private ILogger<LockManager> Logger { get; }
		public LockManager(ILoggerFactory loggerFactory, IGpioPinFactory gpioPinFactory, IOptions<LockOptions> lockOptions, LockDbContext lockDbContext)
		{
			IdToPin = new Dictionary<string, Lock>();
			Logger = loggerFactory.CreateLogger<LockManager>();
			LockDbContext = lockDbContext;
			Initialize(lockOptions.Value, gpioPinFactory, lockDbContext);
		}

		private void Initialize(LockOptions lockOptions, IGpioPinFactory gpioPinFactory, LockDbContext lockDbContext)
		{
			foreach (LockConfiguration lockConfiguration in lockOptions.LockConfigurations)
			{
				LockState state = lockDbContext.Locks.Find(lockConfiguration.Id);
				LockMode mode = state?.Mode ?? LockMode.Lock;
				Lock @lock = new Lock(lockConfiguration.Id, gpioPinFactory.CreatePin((BcmPin)lockConfiguration.Pin), (LockMode)(-1));
				Set(@lock, mode, "INITIALIZE");
				IdToPin.Add(@lock.Id, @lock);
			}
		}

		public IEnumerable<string> Locks => IdToPin.Keys;

		public Task SetLockAsync(string id, LockMode mode, string reason, DateTime? timeout = null)
		{
			if (!IdToPin.TryGetValue(id, out Lock @lock))
			{
				throw new ArgumentException("Unknown id", nameof(id));
			}

			Set(@lock, mode, reason);
			if (timeout.HasValue)
			{
				@lock.Timeout = timeout.Value;
				ScheduleTimeoutCheck(timeout.Value);
			}

			LockState lockState = new LockState()
			{
				Id = id,
				Mode = mode,
				Timeout = timeout
			};
			lock (LockDbContext)
			{
				if (LockDbContext.Locks.Find(lockState.Id) == null)
				{
					LockDbContext.Add(lockState);
				}
				else
				{
					LockDbContext.Update(lockState);
				}
			}
			return LockDbContext.SaveChangesAsync();
		}

		private void ScheduleTimeoutCheck(DateTime time)
		{

		}

		private void Set(Lock @lock, LockMode mode, string reason)
		{
			@lock.Timeout = null;
			if (@lock.Status != mode)
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
				LockDbContext.SaveChanges();
			}
			else
			{
				Logger.LogInformation("{0} is already in mode {1} ({2})", @lock.Id, mode, reason);
			}
		}

		private Task ToggleAsync(Lock @lock, string reason)
		{
			DateTime now = DateTime.Now;
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
			return LockDbContext.SaveChangesAsync();
		}
	}
}
