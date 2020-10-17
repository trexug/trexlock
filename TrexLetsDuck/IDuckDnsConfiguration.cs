using System;
using System.Collections.Generic;
using System.Text;

namespace TrexLetsDuck
{
	public interface IDuckDnsConfiguration
	{
		public string Token { get; }
		public IEnumerable<string> Domains { get; }
		public string Ip { get; }
	}
}
