using RestSharp;
using Newtonsoft.Json;
using RestSharp.Serializers.NewtonsoftJson;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckDns
{
	public class DuckDnsClient
	{
		public RestClient DnsClient { get; }
		private List<string> Domains { get; }
		private string Token { get; }

		public DuckDnsClient(string token, IEnumerable<string> domains)
		{
			DnsClient = new RestClient("https://www.duckdns.org/update");
			DnsClient.UseNewtonsoftJson();
			Token = token;
			Domains = domains.ToList();
		}

		public void UpdateIp(string ip = null)
		{
			RestRequest request = CreateRequest();
			if (ip != null)
			{
				request.AddParameter("ip", ip, ParameterType.QueryString);
			}
		}

		public void UpdateTxt(string txt = null)
		{
			RestRequest request = CreateRequest();
			if (txt != null)
			{
				request.AddParameter("txt", txt, ParameterType.QueryString);
			}
		}

		private RestRequest CreateRequest()
		{
			RestRequest request = new RestRequest();
			request.AddParameter("domains", String.Join(',', Domains), ParameterType.QueryString);
			request.AddParameter("token", Token, ParameterType.QueryString);
			return request;
		}
	}
}
