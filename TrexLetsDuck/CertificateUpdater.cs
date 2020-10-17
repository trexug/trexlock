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
		private X509Certificate2 Certificate { get; set; }
		private CertificateUpdaterConfiguration CertificateUpdaterConfig { get; }
		private LetsEncryptConfiguration LetsEncryptConfiguration { get; }
		private IPemKeyStorage PemKeyStorage { get; }
		private LetsDuckClient Client { get; }
		private ILogger<CertificateUpdater> Logger { get; }
		public CertificateUpdater(ILoggerFactory loggerFactory, IPemKeyStorage pemKeyStorage, IOptions<CertificateUpdaterConfiguration> certificateUpdaterOptions, IOptions<LetsEncryptConfiguration> letsEncryptOptions, IOptions<DuckDnsConfiguration> duckDnsOptions)
		{
			Logger = loggerFactory.CreateLogger<CertificateUpdater>();

			PemKeyStorage = pemKeyStorage;

			CertificateUpdaterConfig = certificateUpdaterOptions.Value;
			LetsEncryptConfiguration = letsEncryptOptions.Value;
			Client = new LetsDuckClient(loggerFactory, letsEncryptOptions.Value, duckDnsOptions.Value, PemKeyStorage.Key);

			DateTime now = DateTime.Now;
			DateTime midnight = now.Date.AddDays(1);
			TimeSpan timeUntilMidnight = midnight - now;
			Timer = new Timer(Callback, null, (int)timeUntilMidnight.TotalMilliseconds, 24 * 60 * 60 * 1000);

			Certificate = File.Exists(CertificateUpdaterConfig.FilePath) ? new X509Certificate2(CertificateUpdaterConfig.FilePath, LetsEncryptConfiguration.CertificatePassword) : null;

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
			if (Certificate == null || Certificate.NotAfter - CertificateUpdaterConfig.RenewBeforeExpiry <= DateTime.Now)
			{
				Logger.LogInformation("Updating the certificate..");
				byte[] newCertificateBytes = Client.GetCertificateAsync().Result;
				PemKeyStorage.Key = Client.LetsEncryptPemKey;
				X509Certificate2 newCertificate = new X509Certificate2(newCertificateBytes, LetsEncryptConfiguration.CertificatePassword);
				string tempFile = Path.GetTempFileName();
				File.WriteAllBytes(tempFile, newCertificateBytes);
				Logger.LogInformation($"Temporary certificate written to {tempFile}");
				File.Move(tempFile, CertificateUpdaterConfig.FilePath, true);
				Certificate = newCertificate;
				Logger.LogInformation("Certificate updated.");
			}
		}

		public void Dispose()
		{
			Timer.Dispose();
		}
	}
}
