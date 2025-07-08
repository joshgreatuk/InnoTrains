using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Text;
using InnoSupabaseAuthentication.Services;
using InnoTrains.Models.Lobby;
using InnoTrains.Services;
using Microsoft.Net.Http.Headers;

namespace InnoTrains.Endpoints
{
	/// <summary>
	/// The websocket controller for lobby and game instances
	/// - Identify user Id
	/// - Take game Id from route
	/// - Check that user is allowed into game
	/// - Get lobby instance from LobbyService (will be auto created)
	/// - Register to lobby's INetworkEngine and accept websocket connection
	/// - When connection is closed, let INetworkEngine know, this will alert the lobby automagically
	/// </summary>
	[Authorize]
	public class GameWebSocketController(LobbyService lobbyService, AuthenticationService authService)  : Controller
	{
		private readonly LobbyService _lobbyService = lobbyService;
		private readonly AuthenticationService _authService = authService;
		
		private Queue<string> messageQueue = new();

		[Route("ws/game/{lobbyGuid}")]
		public async Task<IActionResult> Get(string lobbyGuid, [FromBody] string joinCode)
		{
			if (!HttpContext.WebSockets.IsWebSocketRequest)
			{
				return BadRequest();
			}

			//Grab lobby information
			LobbyInfo lobbyInfo = _lobbyService.GetLobbyInfo(lobbyGuid);
			if (lobbyInfo == null)
			{
				return NotFound();
			}
			
			//Do authentication
			string userId = await _authService.GetUserId(Request.Headers[HeaderNames.Authorization]
				.ToString().Split(" ").Last());
			
			//Check if player is already in list, let them in
			if (!lobbyInfo.Players.Contains(userId))
			{
				//TODO: Implement max player limit
					
				//Check if join code is correct or lobby is public
				if (lobbyInfo.IsPrivate && joinCode != lobbyInfo.LobbyCode && lobbyInfo.Owner != userId)
				{
					return Unauthorized("Lobby join code is incorrect");
				}

				_lobbyService.AddLobbyPlayer(lobbyInfo.GUID, userId);
			}

			//Grab lobby INetworkEngine
			//If lobby isn't active, start it
			if (lobbyInfo.NetworkEngine == null)
			{
				_lobbyService.StartLobby(lobbyInfo.GUID);
			}

			//Do websocket polling
			using WebSocket websocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
			byte[] buffer = new byte[1024 * 4];
			WebSocketReceiveResult result = await websocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

			while (!result.CloseStatus.HasValue)
			{
				byte[] rawRequest = buffer;
				if (messageQueue.TryDequeue(out string message))
				{
					rawRequest = Encoding.UTF8.GetBytes(message);
				}
				ArraySegment<byte> arraySegment = new(rawRequest, 0, rawRequest.Length);

				await websocket.SendAsync(rawRequest, WebSocketMessageType.Text, true, CancellationToken.None);

				result = await websocket.ReceiveAsync(new ArraySegment<byte>(rawRequest), CancellationToken.None);

				if (result.MessageType != WebSocketMessageType.Text)
				{
					continue;
				}

				string resultMessage = Encoding.UTF8.GetString(rawRequest, 0, result.Count);
				if (resultMessage == string.Empty)
				{
					continue;
				}

				lobbyInfo.NetworkEngine.RecieveMessage(userId, resultMessage);
			}

			await websocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);

			return new EmptyResult();
		}

		public void SendMessage(string data)
		{
			messageQueue.Enqueue(data);
		}
	}
}
