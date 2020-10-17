using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace TrexLetsDuck
{
	public class CertificateUpdater : IDisposable
	{
		public string FilePath { get; }
		private Timer Timer { get; }
		private X509Certificate2 Certificate { get; }
		public TimeSpan RenewBeforeExpiry { get; }
		public CertificateUpdater(string filePath, TimeSpan renewBeforeExpiry)
		{
			FilePath = filePath;
			RenewBeforeExpiry = renewBeforeExpiry;

			DateTime now = DateTime.Now;
			DateTime midnight = now.Date.AddDays(1);
			TimeSpan timeUntilMidnight = midnight - now;
			Timer = new Timer(Callback, null, (int)timeUntilMidnight.TotalMilliseconds, 24 * 60 * 60 * 1000);

			Certificate = File.Exists(filePath) ? new X509Certificate2(filePath) : null;
		}

		private void Callback(object obj)
		{
			CheckUpdateCertificate();
		}

		public void CheckUpdateCertificate()
		{
			if (Certificate.NotAfter - RenewBeforeExpiry >= DateTime.Now)
			{

			}
		}

		public void Dispose()
		{
			Timer.Dispose();
		}
	}
}
