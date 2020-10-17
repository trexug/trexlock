using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TrexLock.Persistence;
using TrexLock.Persistence.Dto;

namespace TrexLock.MidLogging
{
	public class RequestResponseLoggingMiddleware
	{
		private readonly RequestDelegate Next;
		private readonly ILogger Logger;
		private LockDbContext LockDbContext { get; }
		public RequestResponseLoggingMiddleware(RequestDelegate next, ILoggerFactory loggerFactory, LockDbContext lockDbContext)
		{
			Next = next;
			Logger = loggerFactory.CreateLogger<RequestResponseLoggingMiddleware>();
			LockDbContext = lockDbContext;
		}

		public async Task Invoke(HttpContext context)
		{
			context.Request.EnableBuffering();
			SecuritylogDto securitylog;
			using (StreamReader reader = new StreamReader(context.Request.Body, leaveOpen: true))
			{
				string body = await reader.ReadToEndAsync();
				context.Request.Body.Position = 0;
				await Next(context);
				securitylog = new SecuritylogDto
				(
					context.Request.HttpContext.Connection.RemoteIpAddress.ToString(),
					context.Request.Method,
					context.Request.GetDisplayUrl(),
					body,
					context.Response.StatusCode
				);
			}
			await LockDbContext.Semaphore.WaitAsync();
			LockDbContext.Add(securitylog);
			await LockDbContext.SaveChangesAsync();
			LockDbContext.Semaphore.Release();
		}
	}
}
