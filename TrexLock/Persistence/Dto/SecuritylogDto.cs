namespace TrexLock.Persistence.Dto
{
	public class SecuritylogDto
	{
		public int? Id { get; set; }
		public string RequestOrigin { get; set; }
		public string Method { get; set; }
		public string Url { get; set; }
		public string Body { get; set; }
		public int ResponseCode { get; set; }

		public SecuritylogDto(string requestOrigin, string method, string url, string body, int responseCode)
		{
			RequestOrigin = requestOrigin;
			Method = method;
			Url = url;
			Body = body;
			ResponseCode = responseCode;
		}
	}
}
