using System;
using TrexLock.Persistence.Dto;
using Unosquare.RaspberryIO.Abstractions;

namespace TrexLock.Locking
{
	public class Lock
	{
		public const GpioPinValue PIN_ON = GpioPinValue.Low;
		public const GpioPinValue PIN_OFF = GpioPinValue.High;

		private IGpioPin Pin { get; set; }
		private LockMode status;
		public string Id { get; }
		public int PinId => Pin.BcmPinNumber;
		public LockMode Status
		{
			get => status;
			set
			{
				Pin.Write(ToPinState(value));
				status = value;
			}
		}
		public DateTime? Timeout { get; set; }

		public void Toggle()
		{
			switch (Status)
			{
				case LockMode.Lock:
					Status = LockMode.Unlock;
					return;
				case LockMode.Unlock:
					Status = LockMode.Lock;
					return;
			}
		}

		public static GpioPinValue ToPinState(LockMode lockMode)
		{
			switch (lockMode)
			{
				case LockMode.Lock:
					return PIN_ON;
				case LockMode.Unlock:
					return PIN_OFF;
				default:
					throw new ArgumentException("Invalid value", nameof(lockMode));
			}
		}

		public Lock(string id, IGpioPin pin, LockMode status)
		{
			Id = id;
			Pin = pin;
			this.status = status;
		}
	}
}
