using System.Text.Json;
using InnoTrains.Models.Game;
using InnoTrains.Models.Game.Networking;
using InnoTrains.Models.Lobby;
using InnoTrains.Services.Data;
using InnoTrains.Services.Game.Networking;
using Microsoft.Extensions.Options;

namespace InnoTrains.Services
{
	[NetworkEvent("lobby")]
	public class LobbyService(ILobbyDataProvider lobbyDataProvider, JsonSerializerOptions serializerOptions,
		IOptions<WebsocketEngineOptions> websocketEngineOptions, IOptions<EngineOptions> engineOptions) : IStoppable
	{
		private readonly ILobbyDataProvider _lobbyDataProvider = lobbyDataProvider;
		private readonly JsonSerializerOptions _serializerOptions = serializerOptions;
		private readonly WebsocketEngineOptions _websocketEngineOptions = websocketEngineOptions.Value;
		private readonly EngineOptions _engineOptions = engineOptions.Value;
		
		public void Stop()
		{
			foreach (LobbyInfo lobby in _lobbyDataProvider.GetLobbyEnumerable())
			{
				FullLobbyShutdown(lobby);
			}
		}
		
		public LobbyInfo? GetLobbyInfo(string lobbyId)
		{
			return _lobbyDataProvider.LoadLobby(lobbyId, true);
		}

		public LobbyInfo[] GetPublicLobbies()
		{
			return _lobbyDataProvider.GetPublicLobbies();
		}

		public LobbyInfo[] GetUserLobbies(string userGuid, bool isOwner)
		{
			IEnumerable<LobbyInfo> lobbies = _lobbyDataProvider.GetLobbyEnumerable();
			lobbies = lobbies.Where(x => x.Players.Contains(userGuid));
			if (isOwner)
			{
				lobbies = lobbies.Where(x => x.Owner == userGuid);
			}

			return lobbies.ToArray();
		}

		public LobbyInfo CreateLobby(string lobbyName, bool isPrivate, string ownerGuid, GameOptions options)
		{
			LobbyInfo newLobby = new LobbyInfo();
			newLobby.Name = lobbyName;
			newLobby.Owner = ownerGuid;
			newLobby.IsPrivate = isPrivate;
			newLobby.GameOptions = options;
			
			//TODO: Create lobby join code
			
			newLobby.Players.Add(ownerGuid);

			_lobbyDataProvider.SaveLobby(newLobby);

			return newLobby;
		}

		public void DeleteLobby(string lobbyGuid)
		{
			//We can assume the LobbyController has done necessary checks
			LobbyInfo lobbyInfo = _lobbyDataProvider.LoadLobby(lobbyGuid, true);

			FullLobbyShutdown(lobbyInfo);

			_lobbyDataProvider.DeleteLobby(lobbyGuid);
		}

		public void UpdateLobby(string lobbyGuid, GameOptions options, bool isPrivate)
		{
			//Send update event to lobby websocket users with new lobbyinfo
			LobbyInfo lobbyInfo = _lobbyDataProvider.LoadLobby(lobbyGuid, true);
			lobbyInfo.GameOptions = options;
			lobbyInfo.IsPrivate = isPrivate;
			_lobbyDataProvider.SaveLobby(lobbyInfo);

			OnLobbyUpdated(lobbyInfo);
		}

		public void FullLobbyShutdown(LobbyInfo lobbyInfo)
		{
			if (lobbyInfo.GameEngine != null)
			{
				//Stop game engine
				ShutdownEngine(lobbyInfo.GUID);
				lobbyInfo.GameEngine = null;
			}
			
			if (lobbyInfo.NetworkEngine != null)
			{
				//Now stop network engine, which will disconnect players
				lobbyInfo.NetworkEngine.Shutdown("The owner of this lobby has deleted it");
				lobbyInfo.NetworkEngine = null;
			}

			OnLobbyUpdated(lobbyInfo);
		}

		public void AddLobbyPlayer(string lobbyGuid, string playerGuid)
		{
			LobbyInfo lobbyInfo = GetLobbyInfo(lobbyGuid);
			if (!lobbyInfo.Players.Contains(playerGuid))
			{
				lobbyInfo.Players.Add(playerGuid);
				OnLobbyUpdated(lobbyInfo);
			}
		}
		
		/// <summary>
		/// Start the lobby NetworkEngine, should be fast so we can hang on this
		/// </summary>
		public void StartLobby(string lobbyGuid)
		{
			LobbyInfo lobbyInfo = _lobbyDataProvider.LoadLobby(lobbyGuid,  true);
			if (lobbyInfo.NetworkEngine != null)
			{
				return;
			}

			WebsocketEngine newEngine = new(_websocketEngineOptions, _serializerOptions, lobbyGuid);
			lobbyInfo.NetworkEngine = newEngine;
			lobbyInfo.State = LobbyInfo.LobbyState.Initialized;
			
			newEngine.RegisterNetworkEvents(this);

			OnLobbyUpdated(lobbyInfo);
		}

		/// <summary>
		/// Start the lobby's engine, this could take time, set status to loading and update clients
		/// </summary>
		public void StartEngine(string lobbyGuid)
		{
			
		}

		public void RestartEngine(string lobbyGuid)
		{
			ShutdownEngine(lobbyGuid);
			StartEngine(lobbyGuid);
		}

		public void PauseEngine(string lobbyGuid)
		{
			
		}

		public void UnpauseEngine(string lobbyGuid)
		{
			
		}

		public void ShutdownEngine(string lobbyGuid)
		{
			
		}

		[NetworkEvent("client_disconnected", true)]
		public void OnLobbyClientDisconnected(string lobbyGuid, string senderGuid, string userGuid)
		{
			//Check if we should pause or stop the game, or whether to close the lobby
			LobbyInfo lobbyInfo = _lobbyDataProvider.LoadLobby(lobbyGuid, true);
			if (lobbyInfo.NetworkEngine.GetConnectedClientCount() > 0
			    || lobbyInfo.GameOptions.LobbyEmptyAction is GameOptions.EmptyAction.None)
			{
				return;
			}
			
			switch (lobbyInfo.GameOptions.LobbyEmptyAction)
			{
				case GameOptions.EmptyAction.Pause:
				{
					if (lobbyInfo.GameEngine != null)
					{
						PauseEngine(lobbyGuid);
					}
					break;
				}
				case GameOptions.EmptyAction.Shutdown:
				{
					FullLobbyShutdown(lobbyInfo);
					break;
				}
			}
		}

		public void OnLobbyUpdated(LobbyInfo lobbyInfo)
		{
			if (lobbyInfo.NetworkEngine == null)
			{
				return;
			}
			
			string lobbyInfoJson = JsonSerializer.Serialize(lobbyInfo, _serializerOptions);
			lobbyInfo.NetworkEngine.BroadcastMessage("updated", lobbyInfoJson);
		}
	}
}
