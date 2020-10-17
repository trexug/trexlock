using DuckDns;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace TrexLetsDuck
{
	public class IpUpdater
	{
		public Timer Timer { get; }
		private DuckDnsClient DuckDnsClient { get; }
		private ILogger<IpUpdater> Logger { get; }
		public IpUpdater(ILoggerFactory loggerFactory, IOptions<IpUpdaterConfiguration> options, IOptions<DuckDnsConfiguration> duckDnsOptions)
		{
			Logger = loggerFactory.CreateLogger<IpUpdater>();
			DuckDnsClient = new DuckDnsClient(duckDnsOptions.Value);

			Timer = new Timer(Callback, null, TimeSpan.Zero, options.Value.UpdateInterval);
			Logger.LogInformation($"{nameof(IpUpdater)} running");
		}

		private void Callback(object state)
		{
			Logger.LogTrace("Updating ip..");
			if (DuckDnsClient.UpdateIpAsync().Result)
			{
				Logger.LogTrace("Ip updated.");
			}
			else
			{
				Logger.LogError("Failed to update ip.");
			}

		}
	}
}
