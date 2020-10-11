using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrexLock.Locking;

namespace TrexLock.Api.Dto
{
	public class LockCommand : AuthorizedCommand
	{
		public LockMode Action { get; set; }
		public int? Timeout { get; set; }
	}
}
