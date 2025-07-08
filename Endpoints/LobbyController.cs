using InnoTrains.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InnoTrains.Endpoints
{
	[Authorize]
	public class LobbyController : Controller
	{
		private readonly LobbyService LobbyService;

		public LobbyController(LobbyService lobbyService)
		{
			LobbyService = lobbyService;
		}

		[HttpGet]
		public IActionResult GetPublicLobbies()
		{
			return Ok();
		}

		[HttpPost]
		public IActionResult CreateLobby()
		{
			return Ok();
		}

		[HttpPost]
		public IActionResult UpdateLobby()
		{
			return Ok();
		}

		[HttpPost]
		public IActionResult DeleteLobby()
		{
			return Ok();
		}
	}
}
