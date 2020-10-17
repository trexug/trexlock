using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Threading;
using TrexLock.Persistence.Dto;

namespace TrexLock.Persistence
{
	public class LockDbContext : DbContext
	{
		private const string InitialDatabase = "trexlock.db.sample";
#if DEBUG
		public const string FilePath = "trexlock.db";
#else
		public const string FilePath = "/data/trexlock.db";
#endif
		public SemaphoreSlim Semaphore { get; }
		public LockDbContext() : base()
		{
			Semaphore = new SemaphoreSlim(1);
		}

		protected override void OnConfiguring(DbContextOptionsBuilder options)
		{
			if (!File.Exists(FilePath))
			{
				File.Copy(InitialDatabase, FilePath);
			}
			options.UseSqlite($"Data Source={FilePath}");
		}

		public DbSet<PinLogDto> PinLogs { get; set; }
		public DbSet<LockDto> Locks { get; set; }
		public DbSet<KeyDto> Keys { get; set; }
		public DbSet<SecuritylogDto> Securitylogs { get; set; }

	}
}
