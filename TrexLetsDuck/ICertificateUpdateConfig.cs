using System;
using System.Collections.Generic;
using System.Text;

namespace TrexLetsDuck
{
	public interface ICertificateUpdateConfig
	{
		public TimeSpan RenewalBeforeExpiration { get; }
		public string CertificateFilePath { get; }
	}
}
