using Microsoft.Extensions.Logging;
using TrexLetsDuck;
using TrexLock.Persistence.Dto;

namespace TrexLock.Persistence
{
	public class PemKeyStorage : IPemKeyStorage
	{
		private const string PemKey = "PemKey";
		public string Key
		{
			get => LockDbContext.Keys.Find(PemKey)?.Value;
			set
			{
				KeyDto keyDto = LockDbContext.Keys.Find(PemKey);
				if (keyDto == null)
				{
					keyDto = new KeyDto()
					{
						Id = PemKey,
						Value = value
					};
				LockDbContext.Add(keyDto);
				}
				else
				{
					keyDto.Value = value;
					LockDbContext.Update(keyDto);
				}
				LockDbContext.SaveChanges();
			}
		}
		private LockDbContext LockDbContext { get; }
		public PemKeyStorage(LockDbContext lockDbContext)
		{
			LockDbContext = lockDbContext;
		}
	}
}
