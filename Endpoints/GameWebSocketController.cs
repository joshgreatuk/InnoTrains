using InnoTrains.Services.Game.Networking;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Text;

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
	public class GameWebSocketController : Controller
	{
		private readonly IServiceProvider Services;
		private Queue<string> messageQueue = new();

		public GameWebSocketController(IServiceProvider services) 
		{
			Services = services;
		}

		[Route("ws/game/{id}")]
		public async Task<IActionResult> Get(string id)
		{
			if (!HttpContext.WebSockets.IsWebSocketRequest)
			{
				return BadRequest();
			}

			//TODO: Do authentication

			//TODO: Grab lobby information

			//TODO: Grab lobby INetworkEngine

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

				//networkEngine.RecieveMessage(userId, resultMessage);
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
