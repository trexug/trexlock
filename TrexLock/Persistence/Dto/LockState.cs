using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrexLock.Locking;

namespace TrexLock.Persistence.Dto
{
	public class LockState
	{
		public string Id { get; set; }
		public LockMode Mode { get; set; }
		public DateTime? Timeout { get; set; }
	}
}
