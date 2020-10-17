using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TrexLock
{
	public class Program
	{
		public const int Port = 443;
		public static X509Certificate2 Certificate { get; set; } = new X509Certificate2();
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
							options.ListenAnyIP(Port, listenOptions =>

								listenOptions.UseHttps(httpsOptions =>
									httpsOptions.ServerCertificateSelector = (context, dnsName) =>
										Certificate
								)
							)
					)
					.UseStartup<Startup>();
				});
	}
}
