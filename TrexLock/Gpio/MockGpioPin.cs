using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unosquare.RaspberryIO.Abstractions;

namespace TrexLock.Gpio
{
	public class MockGpioPin : IGpioPin
	{
		public MockGpioPin(BcmPin id)
		{
			BcmPin = id;
			PinMode = GpioPinDriveMode.Input;
		}
		public bool OutputValue { get; set; }
		public BcmPin BcmPin { get; }

		public int BcmPinNumber => (int)BcmPin;

		public int PhysicalPinNumber => throw new NotImplementedException();

		public GpioHeader Header => throw new NotImplementedException();

		public GpioPinDriveMode PinMode { get; set; }
		public GpioPinResistorPullMode InputPullMode { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public bool Value
		{
			get => throw new NotImplementedException();
			set => Write(value);
		}

		public bool Read()
		{
			throw new NotImplementedException();
		}

		public void RegisterInterruptCallback(EdgeDetection edgeDetection, Action callback)
		{
			throw new NotImplementedException();
		}

		public void RegisterInterruptCallback(EdgeDetection edgeDetection, Action<int, int, uint> callback)
		{
			throw new NotImplementedException();
		}

		public void RemoveInterruptCallback(EdgeDetection edgeDetection, Action callback)
		{
			throw new NotImplementedException();
		}

		public void RemoveInterruptCallback(EdgeDetection edgeDetection, Action<int, int, uint> callback)
		{
			throw new NotImplementedException();
		}

		public bool WaitForValue(GpioPinValue status, int timeOutMillisecond)
		{
			throw new NotImplementedException();
		}

		public void Write(bool value)
		   => Write(value ? GpioPinValue.High : GpioPinValue.Low);

		public void Write(GpioPinValue value)
		{
			lock (this)
			{
				if (PinMode != GpioPinDriveMode.Output)
				{
					throw new InvalidOperationException(
						$"Unable to write to pin {BcmPinNumber} because operating mode is {PinMode}."
						+ $" Writes are only allowed if {nameof(PinMode)} is set to {GpioPinDriveMode.Output}");
				}

				OutputValue = value == GpioPinValue.High;
			}
		}
	}
}
