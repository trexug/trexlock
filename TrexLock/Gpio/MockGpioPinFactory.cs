using Unosquare.RaspberryIO.Abstractions;

namespace TrexLock.Gpio
{
	public class MockGpioPinFactory : IGpioPinFactory
	{
		public IGpioPin CreatePin(BcmPin id, GpioPinDriveMode mode = GpioPinDriveMode.Output)
		{
			var pin = new MockGpioPin(id);
			pin.PinMode = mode;
			return pin;
		}
	}
}
