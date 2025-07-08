using InnoSupabaseAuthentication.Services;
using InnoTrains.Models.Game;
using InnoTrains.Models.Lobby;
using InnoTrains.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace InnoTrains.Endpoints
{
	[Authorize]
	public class LobbyController(LobbyService lobbyService, AuthenticationService authService) : Controller
	{
		private readonly LobbyService _lobbyService = lobbyService;
		private readonly AuthenticationService _authService = authService;

		[HttpGet]
		public IActionResult GetPublicLobbies()
		{
			try
			{
				return Ok(_lobbyService.GetPublicLobbies());
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				return Conflict(ex.Message);
			}
		}

		[HttpGet]
		public async Task<IActionResult> GetUserLobbies()
		{
			try
			{
				//Grab user guid
				string userGuid = await _authService.GetUserId(Request.Headers[HeaderNames.Authorization].ToString().Split(" ").Last());
				return Ok(_lobbyService.GetUserLobbies(userGuid, false));
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				return Conflict(ex.Message);
			}
		}
		
		[HttpGet]
		public async Task<IActionResult> GetOwnedLobbies()
		{
			try
			{
				//Grab user guid
				string userGuid = await _authService.GetUserId(Request.Headers[HeaderNames.Authorization]
					.ToString().Split(" ").Last());
				return Ok(_lobbyService.GetUserLobbies(userGuid, true));
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				return Conflict(ex.Message);
			}
		}

		[HttpPost]
		public async Task<IActionResult> CreateLobby([FromBody] CreateLobbyRequest request)
		{
			try
			{
				//Grab owner guid
				string ownerGuid = await _authService.GetUserId(Request.Headers[HeaderNames.Authorization]
					.ToString().Split(" ").Last());
				LobbyInfo newLobby = _lobbyService.CreateLobby(request.LobbyName, request.IsPrivate, ownerGuid, request.GameOptions);

				return RequestLobbyConnect(newLobby.GUID);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				return Conflict(ex.Message);
			}
		}

		[HttpPost]
		public async Task<IActionResult> UpdateLobby(UpdateLobbyRequest request)
		{
			try
			{
				//Grab owner guid
				string ownerGuid = await _authService.GetUserId(Request.Headers[HeaderNames.Authorization]
					.ToString().Split(" ").Last());

				//Check this is the lobby owner
				LobbyInfo? lobby = _lobbyService.GetLobbyInfo(request.LobbyGuid);
				if (lobby == null)
				{
					return NotFound();
				}

				if (lobby.Owner != ownerGuid)
				{
					return Unauthorized();
				}

				_lobbyService.UpdateLobby(request.LobbyGuid, request.GameOptions, request.IsPrivate);

				return Ok();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				return Conflict(ex.Message);
			}
		}

		[HttpPost]
		public async Task<IActionResult> DeleteLobby([FromBody] string lobbyGuid)
		{
			try
			{
				//Grab owner guid
				string ownerGuid = await _authService.GetUserId(Request.Headers[HeaderNames.Authorization]
					.ToString().Split(" ").Last());

				LobbyInfo lobbyInfo = _lobbyService.GetLobbyInfo(lobbyGuid);
				if (lobbyInfo == null)
				{
					return NotFound();
				}

				if (lobbyInfo.Owner != ownerGuid)
				{
					return Unauthorized();
				}

				_lobbyService.DeleteLobby(lobbyGuid);
				
				return Ok();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				return Conflict(ex.Message);
			}
		}

		[HttpPost]
		public IActionResult RequestLobbyConnect([FromBody] string lobbyGuid)
		{
			try
			{
				LobbyInfo lobbyInfo = _lobbyService.GetLobbyInfo(lobbyGuid);
				if (lobbyInfo == null)
				{
					return NotFound();
				}
				
				string websocketURL = string.Format("{0}://{1}{2}", 
					HttpContext.Request.Scheme,HttpContext.Request.Host,$"/ws/game/{lobbyInfo.GUID}");

				return Ok(websocketURL);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				return Conflict(ex.Message);
			}
		}
	}
}
