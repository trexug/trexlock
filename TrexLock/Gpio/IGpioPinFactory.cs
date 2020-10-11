using Unosquare.RaspberryIO.Abstractions;

namespace TrexLock.Gpio
{
	public interface IGpioPinFactory
	{
		IGpioPin CreatePin(BcmPin id, GpioPinDriveMode mode = GpioPinDriveMode.Output);
	}
}