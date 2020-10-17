using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TrexLock
{
	public class Program
	{
		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder
					.UseKestrel
					(options =>
							options.Listen(IPAddress.Any, 8223, listenOptions =>
							
								listenOptions.UseHttps(httpsOptions => 
									httpsOptions.ServerCertificateSelector = (context, dnsName) =>
										new X509Certificate2(@"C:\Files\cert", "1234")
								)
							)
					)
					.UseStartup<Startup>();
				});
	}
}
