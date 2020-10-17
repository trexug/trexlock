using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using TrexLetsDuck;

namespace TrexLock
{
	public class CertificateFileWatcher
	{
		public X509Certificate2 Certificate { get; private set; }
		public string FilePath { get; private set; }
		public string CertificatePassword { get; private set; }
		private FileSystemWatcher Watcher { get; }
		private ILogger<CertificateFileWatcher> Logger { get; }
		public event EventHandler<X509Certificate2> CertificateChanged;

		public CertificateFileWatcher(ILoggerFactory loggerFactory, IOptions<CertificateUpdaterConfiguration> certificateOptions, IOptions<LetsEncryptConfiguration> letsEncryptOptions)
			: this(loggerFactory, certificateOptions.Value.FilePath, letsEncryptOptions.Value.CertificatePassword)
		{

		}

		public CertificateFileWatcher(ILoggerFactory loggerFactory, string filePath, string certificatePassword)
		{
			Logger = loggerFactory.CreateLogger<CertificateFileWatcher>();
			FilePath = filePath;
			CertificatePassword = certificatePassword;

			Watcher = new FileSystemWatcher();
			Watcher.Path = Path.GetDirectoryName(FilePath);
			Watcher.Filter = Path.GetFileName(FilePath);
			Watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
			Watcher.Changed += Watcher_Changed;
			Watcher.Created += Watcher_Changed;
			Watcher.Renamed += Watcher_Changed;
			Watcher.EnableRaisingEvents = true;

			Certificate = LoadFromFile();
		}

		private void Watcher_Changed(object sender, FileSystemEventArgs e)
		{
			Logger.LogInformation("Certificate change detected");
			Certificate = LoadFromFile();
			CertificateChanged?.Invoke(this, Certificate);
		}

		private X509Certificate2 LoadFromFile()
		{
			if (File.Exists(FilePath))
			{
				byte[] bytes = null;
				for (int i = 0; bytes == null && i < 10; i++)
				{
					try
					{
						bytes = File.ReadAllBytes(FilePath);
					}
					catch (IOException)
					{
						Thread.Sleep(1000);
					}
				}
				
				X509Certificate2 certificate2 = new X509Certificate2(bytes, CertificatePassword);
				Logger.LogInformation("Loaded certificate from file");
				return certificate2;
			}
			return null;
		}
	}
}
