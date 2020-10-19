using System;
using System.Threading;
using TrexLock.Persistence.Dto;
using Unosquare.RaspberryIO.Abstractions;

namespace TrexLock.Locking
{
	public class Lock
	{
		public const GpioPinValue PIN_ON = GpioPinValue.High;
		public const GpioPinValue PIN_OFF = GpioPinValue.Low;

		private IGpioPin Pin { get; set; }
		private LockState state;
		public string Id { get; }
		public int PinId => Pin.BcmPinNumber;
		public SemaphoreSlim Semaphore { get; }
		public LockState State
		{
			get => state;
			set
			{
				Pin.Write(ToPinState(value));
				state = value;
			}
		}
		public DateTime? Timeout { get; set; }

		public static GpioPinValue ToPinState(LockState lockMode)
		{
			switch (lockMode)
			{
				case LockState.Locked:
					return PIN_ON;
				case LockState.Unlocked:
					return PIN_OFF;
				default:
					throw new ArgumentException("Invalid value", nameof(lockMode));
			}
		}

		public Lock(string id, IGpioPin pin, LockState status)
		{
			Id = id;
			Pin = pin;
			this.state = status;
			Semaphore = new SemaphoreSlim(1);
		}
	}
}
