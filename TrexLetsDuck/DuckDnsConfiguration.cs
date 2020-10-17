using Org.BouncyCastle.Bcpg.OpenPgp;
using System;
using System.Collections.Generic;
using System.Text;

namespace TrexLetsDuck
{
	public class DuckDnsConfiguration
	{
		public const string DuckDns = "DuckDns";
		public string Token { get; set; }
		public IEnumerable<string> Domains { get; set; }
		public string Ip { get; set; }
	}
}
