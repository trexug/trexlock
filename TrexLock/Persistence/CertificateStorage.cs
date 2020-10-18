using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using TrexLetsDuck;
using TrexLock.Persistence.Dto;

namespace TrexLock.Persistence
{
	public class CertificateStorage : ICertificateStorage
	{
		private const string PemKey = "PemKey";
		private const string CertificateKey = "Certificate";
		public string Key
		{
			get => LockDbContext.Keys.Find(PemKey)?.Value;
			set
			{
				LockDbContext.Semaphore.Wait();
				try
				{
					KeyDto keyDto = LockDbContext.Keys.Find(PemKey);
					if (keyDto == null)
					{
						keyDto = new KeyDto()
						{
							Id = PemKey,
							Value = value
						};
						LockDbContext.Add(keyDto);
					}
					else
					{
						keyDto.Value = value;
						LockDbContext.Keys.Update(keyDto);
					}
					LockDbContext.SaveChanges();
				}
				finally
				{
					LockDbContext.Semaphore.Release();
				}
			}
		}
		private LockDbContext LockDbContext { get; }
		public X509Certificate2 Certificate { get; private set; }
		public event EventHandler<X509Certificate2> CertificateChanged;
		private ILogger<CertificateStorage> Logger { get; }
		private LetsEncryptConfiguration LetsEncryptConfiguration { get; }
		public CertificateStorage(ILoggerFactory loggerFactory, LockDbContext lockDbContext, IOptions<LetsEncryptConfiguration> letsEncryptOptions)
		{
			Logger = loggerFactory.CreateLogger<CertificateStorage>();
			LetsEncryptConfiguration = letsEncryptOptions.Value;
			LockDbContext = lockDbContext;
			byte[] bytes;
			LockDbContext.Semaphore.Wait();
			try
			{
				bytes = Decode(LockDbContext.Keys.Find(CertificateKey)?.Value);
			}
			finally
			{
				LockDbContext.Semaphore.Release();
			}
			Certificate = bytes != null ? new X509Certificate2(bytes, LetsEncryptConfiguration.CertificatePassword) : null;
			Logger.LogInformation($"{nameof(CertificateStorage)} is operational");
		}

		public async Task UpdateCertificateAsync(byte[] bytes)
		{
			X509Certificate2 newCertificate = new X509Certificate2(bytes, LetsEncryptConfiguration.CertificatePassword);
			string encoded = Encode(bytes);
			await LockDbContext.Semaphore.WaitAsync();
			try
			{
				KeyDto keyDto = LockDbContext.Keys.Find(CertificateKey);
				if (keyDto == null)
				{
					keyDto = new KeyDto()
					{
						Id = CertificateKey,
						Value = encoded
					};
					LockDbContext.Add(keyDto);
				}
				else
				{
					keyDto.Value = encoded;
					LockDbContext.Update(keyDto);
				}
				LockDbContext.SaveChanges();
				Logger.LogInformation("Certificate stored.");
			}
			finally
			{
				LockDbContext.Semaphore.Release();
			}
			Certificate = newCertificate;
			CertificateChanged?.Invoke(this, Certificate);
			Logger.LogInformation("Certificate updated.");
		}

		private static byte[] Decode(string value)
		{
			if (value == null)
			{
				return null;
			}
			return Convert.FromBase64String(value);
		}

		private static string Encode(byte[] bytes)
		{
			if (bytes == null)
			{
				return null;
			}
			return Convert.ToBase64String(bytes);
		}
	}
}
