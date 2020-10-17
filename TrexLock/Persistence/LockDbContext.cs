using Microsoft.EntityFrameworkCore;
using System.Threading;
using TrexLock.Persistence.Dto;

namespace TrexLock.Persistence
{
	public class LockDbContext : DbContext
	{
		public SemaphoreSlim Semaphore { get; }
		public LockDbContext() : base()
		{
			Semaphore = new SemaphoreSlim(1);
		}

		protected override void OnConfiguring(DbContextOptionsBuilder options)
			=> options.UseSqlite("Data Source=trexlock.db");
		public DbSet<PinLogDto> PinLogs { get; set; }
		public DbSet<LockDto> Locks { get; set; }
		public DbSet<KeyDto> Keys { get; set; }
		public DbSet<SecuritylogDto> Securitylogs { get; set; }

	}
}
