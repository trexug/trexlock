using System.ComponentModel;

namespace TrexLock.Locking
{
	public static class LockStateExtensions
	{
		public static LockState Toggle(this LockState lockState)
		{
			switch (lockState)
			{
				case LockState.Locked:
					return LockState.Unlocked;
				case LockState.Unlocked:
					return LockState.Locked;
				default:
					throw new InvalidEnumArgumentException(nameof(lockState), (int)lockState, typeof(LockState));
			}
		}
	}
}
