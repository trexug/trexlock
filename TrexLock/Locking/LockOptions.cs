using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TrexLock.Locking
{
	public class LockOptions
	{
		public const string Locking = "Locking";
		public List<LockConfiguration> LockConfigurations { get; set; }
	}
}
