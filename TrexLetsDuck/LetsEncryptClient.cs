using Certes;
using Certes.Acme;
using Certes.Acme.Resource;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TrexLetsDuck
{
	public class LetsEncryptClient
	{
		private readonly static Uri LetsEncryptUri = WellKnownServers.LetsEncryptV2;
		private AcmeContext AcmeContext { get; set; }
		private LetsEncryptConfiguration Configuration { get; }
		public LetsEncryptClient(LetsEncryptConfiguration configuration)
		{
			AcmeContext = null;
			Configuration = configuration;
		}

		public async Task<string> CreateAccountAsync()
		{
			var acmeContext = new AcmeContext(LetsEncryptUri);
			await acmeContext.NewAccount(Configuration.Email, true);
			AcmeContext = acmeContext;
			return acmeContext.AccountKey.ToPem();
		}

		public async Task LoginAsync(string pemKey)
		{
			var accountKey = KeyFactory.FromPem(pemKey);
			var acmeContext = new AcmeContext(LetsEncryptUri, accountKey);
			await acmeContext.Account();
			AcmeContext = acmeContext;
		}

		public async Task<byte[]> GetCertificateAsync(Action<string> setDnsTxt)
		{
			IOrderContext order = await AcmeContext.NewOrder(new[] { Configuration.CertificateIdentifier });
			var authz = (await order.Authorizations()).First();
			var dnsChallenge = await authz.Dns();
			var dnsTxt = AcmeContext.AccountKey.DnsTxt(dnsChallenge.Token);
			var privateKey = KeyFactory.NewKey(KeyAlgorithm.ES256);
			setDnsTxt(dnsTxt);
			Challenge challenge = null;
			for (int i = 0; challenge?.Status != ChallengeStatus.Valid && i < 10; i++)
			{
				challenge = await dnsChallenge.Validate();
				if (challenge.Status == ChallengeStatus.Valid)
				{
					break;
				}
				Thread.Sleep(1000);
			}
			var cert = await order.Generate(Configuration.CsrInfo, privateKey);
			var pfxBuilder = cert.ToPfx(privateKey);
			return pfxBuilder.Build(Configuration.CertificateName, Configuration.CertificatePassword);
		}
	}
}
