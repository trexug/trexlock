using Certes;
using Certes.Acme;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TrexLetsDuck;

namespace TrexLetsDuckConsole
{
	class Program
	{
		private const string Key = "-----BEGIN EC PRIVATE KEY-----\r\nMHcCAQEEIEJJiaP4GpUetLgXm9ubQAdWnIJm7DCDMLeI90ZFeqYuoAoGCCqGSM49\r\nAwEHoUQDQgAEE/V7Lcl8Q5++mi/2qpLawyky9KGGp9hIJb187WKldfJ70EbbTxIW\r\n6nY2GMNg/44cpkn9rqbbZXUirqB0Q5X0Mw==\r\n-----END EC PRIVATE KEY-----";

		static void Main(string[] args)
		{
			Run().Wait();
		}

		private static async Task Run()
		{
			LetsDuckClient client = new LetsDuckClient();
			await client.LoginAsync(Key);
			CsrInfo csrInfo = new CsrInfo()
			{
				CountryName = "Denmark",
				State = "Nordjylland",
				CommonName = "trlg.duckdns.org",
				Locality = "Aalborg",
				Organization = "Trex",
				OrganizationUnit = "TrexLock"
			};
			byte[] cert = await client.GetCertificateAsync("TrexLockCert", "1234", "7925f751-1e90-4f79-a4f5-2984e5bcaf29", "trlg", "trlg.duckdns.org", csrInfo);
			File.WriteAllBytes(@"C:\files\cert", cert);
			Console.WriteLine();
		}
	}
}
