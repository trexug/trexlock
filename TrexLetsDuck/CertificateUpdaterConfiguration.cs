using System;
using System.Collections.Generic;
using System.Text;

namespace TrexLetsDuck
{
	public class CertificateUpdaterConfiguration
	{
		public const string CertificateUpdater = "CertificateUpdater";
		public string FilePath { get; set; }
		public TimeSpan RenewBeforeExpiry { get; set; }
	}
}
