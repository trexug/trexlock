using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TrexLock.MidLogging
{
	public class RequestResponseLoggingMiddleware
	{
		private readonly RequestDelegate Next;
		private readonly ILogger Logger;
		public RequestResponseLoggingMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
		{
			Next = next;
			Logger = loggerFactory.CreateLogger<RequestResponseLoggingMiddleware>();
		}

		public async Task Invoke(HttpContext context)
		{
			using (StreamReader reader = new StreamReader(context.Request.Body))
			{
				
			}
			await Next(context);
			//code dealing with the response
		}
	}
}
