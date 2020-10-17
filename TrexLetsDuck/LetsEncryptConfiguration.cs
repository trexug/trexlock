using Certes;
using System;
using System.Collections.Generic;
using System.Security;
using System.Text;

namespace TrexLetsDuck
{
	public class LetsEncryptConfiguration
	{
		public const string LetsEncrypt = "LetsEncrypt";
		public string Email { get; set; }
		public string CertificateName { get; set; }
		public string CertificatePassword { get; set; }
		public string CertificateIdentifier { get; set; }
		public CsrInfo CsrInfo { get; set; }
	}
}
