using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TrexLetsDuck;
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
			services
			.AddOptions<AuthOptions>().Bind(Configuration.GetSection(AuthOptions.Auth)).Services
			.AddOptions<LockOptions>().Bind(Configuration.GetSection(LockOptions.Locking)).Services
			.AddOptions<CertificateUpdaterConfiguration>().Bind(Configuration.GetSection(CertificateUpdaterConfiguration.CertificateUpdater)).Services
			.AddOptions<DuckDnsConfiguration>().Bind(Configuration.GetSection(DuckDnsConfiguration.DuckDns)).Services
			.AddOptions<IpUpdaterConfiguration>().Bind(Configuration.GetSection(IpUpdaterConfiguration.IpUpdater)).Services
			.AddOptions<LetsEncryptConfiguration>().Bind(Configuration.GetSection(LetsEncryptConfiguration.LetsEncrypt)).Services
			.AddLogging(lb => lb.AddConsole().SetMinimumLevel(LogLevel.Trace))
			.AddEntityFrameworkSqlite()
			.AddDbContext<LockDbContext>(ServiceLifetime.Singleton)
			.AddSingleton<LockManager, LockManager>()
			.AddSingleton<IGpioPinFactory, MockGpioPinFactory>()
			.AddSingleton<CertificateUpdater>()
			.AddSingleton<IpUpdater>()
			.AddSingleton<IPemKeyStorage, PemKeyStorage>()
			.AddSingleton<CertificateFileWatcher>()
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
			app.ApplicationServices.GetService<IpUpdater>();
			app.ApplicationServices.GetService<CertificateUpdater>();
			CertificateFileWatcher watcher = app.ApplicationServices.GetService<CertificateFileWatcher>();
			Program.Certificate = watcher.Certificate;
			watcher.CertificateChanged += (s, e) =>
			{
				if (e != null)
				{
					Program.Certificate = e;
				}
			};
		}

	}
}
