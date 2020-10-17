using Certes;
using System;
using System.Collections.Generic;
using System.Security;
using System.Text;

namespace TrexLetsDuck
{
	public interface ILetsEncryptConfiguration
	{
		public string Email { get; }
		public string CertificateName { get; }
		public string CertificatePassword { get; }
		public string CertificateIdentifier { get; }
		public CsrInfo CsrInfo { get; }
	}
}
