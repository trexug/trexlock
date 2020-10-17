using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TrexLock.Api;
using TrexLock.Api.Dto;
using TrexLock.Locking;
using TrexLock.Persistence;

namespace TrexLock.Controllers
{
	[Route("api/ifttt/1/lock")]
	[ApiController]
	public class LockController : ControllerBase
	{
		private LockManager LockManager { get; }
		private AuthOptions AuthOptions { get; }

		private LockDbContext LockDbContext { get; }

		public LockController(LockManager lockManager, IOptions<AuthOptions> authOptions, LockDbContext lockDbContext)
		{
			LockManager = lockManager;
			AuthOptions = authOptions.Value;
			LockDbContext = lockDbContext;
		}

		[HttpPost("{id}")]
		public async Task<ActionResult> PostCommandAsync(string id, [FromBody] LockCommand command)
		{
			const string SetReason = "IFTTT Set";
			const string ToggleReason = "IFTTT Toggle";

			ActionResult result;

			if (!IsAuthorized(command))
			{
				result = Forbid();
			}
			else
			{
				DateTime now = DateTime.Now;
				DateTime? timeout = null;
				if (command.Timeout.HasValue)
				{
					timeout = now + TimeSpan.FromSeconds(command.Timeout.Value);
				}

				switch (command.Action)
				{
					case LockAction.Lock:
						await LockManager.SetLockAsync(id, LockState.Locked, SetReason, timeout);
						break;
					case LockAction.Unlock:
						await LockManager.SetLockAsync(id, LockState.Unlocked, SetReason, timeout);
						break;
					case LockAction.Toggle:
						await LockManager.ToggleLockAsync(id, ToggleReason);
						break;
					default:
						return BadRequest();
				}
				
				result = Ok();
			}

			return result;
		}

		private bool IsAuthorized(AuthorizedCommand command)
		{
			return command.Token == AuthOptions.Token;
		}

	}
}
