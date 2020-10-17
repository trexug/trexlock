using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using System;
using System.Net;
using System.Threading.Tasks;
using TrexLetsDuck;

namespace DuckDns
{
	public class DuckDnsClient
	{
		public const string DuckDnsUpdateUrl = "https://www.duckdns.org/update";
		private RestClient DnsClient { get; }
		private DuckDnsConfiguration DuckDnsConfiguration { get; }
		public DuckDnsClient(DuckDnsConfiguration duckDnsConfiguration)
		{
			DuckDnsConfiguration = duckDnsConfiguration;
			DnsClient = new RestClient(DuckDnsUpdateUrl);
			DnsClient.UseNewtonsoftJson();
		}

		public Task<bool> UpdateIpAsync(string ip = null)
		{
			RestRequest request = CreateRequest();
			if (ip != null)
			{
				request.AddParameter("ip", ip, ParameterType.QueryString);
			}
			request.Method = Method.GET;
			return DnsClient.ExecuteAsync(request).ContinueWith(t => t.Result.StatusCode == HttpStatusCode.OK);
		}

		public Task<bool> UpdateTxtAsync(string txt)
		{
			RestRequest request = CreateRequest();
			
			request.AddParameter("txt", txt, ParameterType.QueryString);
			request.Method = Method.GET;
			return DnsClient.ExecuteAsync(request).ContinueWith(t => t.Result.StatusCode == HttpStatusCode.OK);
		}

		private RestRequest CreateRequest()
		{
			RestRequest request = new RestRequest();
			request.AddParameter("domains", String.Join(',', DuckDnsConfiguration.Domains), ParameterType.QueryString);
			request.AddParameter("token", DuckDnsConfiguration.Token, ParameterType.QueryString);
			return request;
		}
	}
}
