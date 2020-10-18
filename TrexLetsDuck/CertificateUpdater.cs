using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace TrexLetsDuck
{
	public class CertificateUpdater : IDisposable
	{
		public string FilePath { get; }
		private Timer Timer { get; }
		private CertificateUpdaterConfiguration CertificateUpdaterConfig { get; }
		private LetsEncryptConfiguration LetsEncryptConfiguration { get; }
		private ICertificateStorage CertificateStorage { get; }
		private LetsDuckClient Client { get; }
		private ILogger<CertificateUpdater> Logger { get; }
		public CertificateUpdater(ILoggerFactory loggerFactory, ICertificateStorage pemKeyStorage, IOptions<CertificateUpdaterConfiguration> certificateUpdaterOptions, IOptions<LetsEncryptConfiguration> letsEncryptOptions, IOptions<DuckDnsConfiguration> duckDnsOptions)
		{
			Logger = loggerFactory.CreateLogger<CertificateUpdater>();

			CertificateStorage = pemKeyStorage;

			CertificateUpdaterConfig = certificateUpdaterOptions.Value;
			LetsEncryptConfiguration = letsEncryptOptions.Value;
			Client = new LetsDuckClient(loggerFactory, letsEncryptOptions.Value, duckDnsOptions.Value, CertificateStorage.Key);

			DateTime now = DateTime.Now;
			DateTime midnight = now.Date.AddDays(1);
			TimeSpan timeUntilMidnight = midnight - now;
			Timer = new Timer(Callback, null, (int)timeUntilMidnight.TotalMilliseconds, 24 * 60 * 60 * 1000);

			Logger.LogInformation($"{nameof(CertificateUpdater)} running");
			CheckUpdateCertificate();
		}

		private void Callback(object obj)
		{
			CheckUpdateCertificate();
		}

		public void CheckUpdateCertificate()
		{
			Logger.LogTrace("Checking certificate validity");
			if (CertificateStorage.Certificate == null || CertificateStorage.Certificate.NotAfter - CertificateUpdaterConfig.RenewBeforeExpiry <= DateTime.Now)
			{
				Logger.LogInformation("Updating the certificate..");
				byte[] newCertificateBytes = Client.GetCertificateAsync().Result;
				CertificateStorage.Key = Client.LetsEncryptPemKey;
				X509Certificate2 newCertificate = new X509Certificate2(newCertificateBytes, LetsEncryptConfiguration.CertificatePassword);
				string tempFile = Path.GetTempFileName();
				File.WriteAllBytes(tempFile, newCertificateBytes);
				Logger.LogInformation($"Temporary certificate written to {tempFile}");
				var parent = new FileInfo(CertificateUpdaterConfig.FilePath).Directory;
				if (!parent.Exists)
				{
					parent.Create();
				}
				File.Move(tempFile, CertificateUpdaterConfig.FilePath, true);
				CertificateStorage.UpdateCertificateAsync(newCertificateBytes).Wait();
				Logger.LogInformation("Certificate updated.");
			}
		}

		public void Dispose()
		{
			Timer.Dispose();
		}
	}
}
