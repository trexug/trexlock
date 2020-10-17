using System;
using System.Collections.Generic;
using System.Text;

namespace TrexLetsDuck
{
	public interface IIpUpdateConfig
	{
		public TimeSpan UpdateInterval { get; }
	}
}
