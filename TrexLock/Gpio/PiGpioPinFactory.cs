using Unosquare.RaspberryIO;
using Unosquare.RaspberryIO.Abstractions;
using Unosquare.WiringPi;

namespace TrexLock.Gpio
{
	public class PiGpioPinFactory : IGpioPinFactory
	{
		public PiGpioPinFactory()
		{
			Pi.Init<BootstrapWiringPi>();
		}
		public IGpioPin CreatePin(BcmPin id, GpioPinDriveMode mode = GpioPinDriveMode.Output)
		{
			IGpioPin pin = Pi.Gpio[id];
			pin.PinMode = mode;
			return pin;
		}
	}
}
