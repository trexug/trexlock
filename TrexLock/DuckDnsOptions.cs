using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TrexLock
{
	public class DuckDnsOptions
	{
		public const string DuckDns = "DuckDns";
		public string Token { get; set; }
		public List<string> Domains { get; set; }
	}
}
