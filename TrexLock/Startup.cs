using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TrexLock.Api;
using TrexLock.Gpio;
using TrexLock.Locking;
using TrexLock.MidLogging;
using TrexLock.Persistence;

namespace TrexLock
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddOptions<AuthOptions>().Bind(Configuration.GetSection(AuthOptions.Auth))
			.Services
			.AddOptions<LockOptions>().Bind(Configuration.GetSection(LockOptions.Locking))
			.Services
			.AddLogging(lb => lb.AddConsole().SetMinimumLevel(LogLevel.Trace))
			.AddEntityFrameworkSqlite()
			.AddDbContext<LockDbContext>(ServiceLifetime.Singleton)
			.AddSingleton<LockManager, LockManager>()
			.AddSingleton<IGpioPinFactory, MockGpioPinFactory>()
			.AddControllers().AddNewtonsoftJson();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseRouting();

			app.UseAuthorization();

			app.UseRequestResponseLogging();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});

			app.ApplicationServices.GetService<LockManager>();
		}

	}
}
