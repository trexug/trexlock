using DuckDns;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace TrexLetsDuck
{
	public class LetsDuckClient
	{
		public string LetsEncryptPemKey { get; private set; }
		private LetsEncryptClient LetsEncryptClient { get; }
		private DuckDnsClient DuckDnsClient { get; }
		private ILogger<LetsDuckClient> Logger { get; }
		public LetsDuckClient(ILoggerFactory loggerFactory, ILetsEncryptConfiguration letsEncryptConfiguration, IDuckDnsConfiguration duckDnsConfiguration, string letsEncryptPemKey = null)
		{
			Logger = loggerFactory.CreateLogger<LetsDuckClient>();

			LetsEncryptClient = new LetsEncryptClient(letsEncryptConfiguration);
			DuckDnsClient = new DuckDnsClient(duckDnsConfiguration);
			LetsEncryptPemKey = letsEncryptPemKey;
		}

		public async Task<byte[]> GetCertificateAsync()
		{
			if (LetsEncryptPemKey == null)
			{
				Logger.LogInformation("Creating account for LetsEncrypt..");
				LetsEncryptPemKey = await LetsEncryptClient.CreateAccountAsync();
			}
			else
			{
				Logger.LogInformation("Logging into LetsEncrypt..");
				await LetsEncryptClient.LoginAsync(LetsEncryptPemKey);
			}
			Logger.LogInformation("Generating LetsEncrypt certificate..");
			return await LetsEncryptClient.GetCertificateAsync(SetDnsTxt);
		}

		private void SetDnsTxt(string dnsTxt)
		{
			Logger.LogInformation("Updating DuckDns TXT record..");
			if (!DuckDnsClient.UpdateTxtAsync(dnsTxt).Result)
			{
				throw new Exception();
			}
		}
	}
}
