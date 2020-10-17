﻿using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TrexLock.Persistence.Dto;

namespace TrexLock.Persistence
{
	public class LockDbContext : DbContext
	{
		public LockDbContext() : base()
		{
			
		}

		protected override void OnConfiguring(DbContextOptionsBuilder options)
			=> options.UseSqlite("Data Source=trexlock.db");
		public DbSet<PinLogDto> PinLogs { get; set; }
		public DbSet<LockDto> Locks { get; set; }

	}
}