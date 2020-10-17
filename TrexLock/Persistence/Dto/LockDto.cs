using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrexLock.Locking;

namespace TrexLock.Persistence.Dto
{
	public class LockDto
	{
		public string Id { get; set; }
		public LockState State { get; set; }
		public DateTime? Timeout { get; set; }
	}
}
