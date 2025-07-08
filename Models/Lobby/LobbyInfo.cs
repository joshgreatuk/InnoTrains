using InnoTrains.Models.Game;
using System.Text.Json.Serialization;
using InnoTrains.Services;
using InnoTrains.Services.Game.Networking;

namespace InnoTrains.Models.Lobby
{
	/// <summary>
	/// LobbyInfo contains lobby information and settings such as permissions, capacities, etc
	/// </summary>
	public class LobbyInfo
	{
		public enum LobbyState
		{
			Closed,
			Initialized,
			Loading,
			Running,
			Paused,
			
		}
		
		#region JSON Fields
		
		/// <summary>
		/// Lobby display name
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// GUID of the lobby
		/// </summary>
		public string GUID { get; set; }

		/// <summary>
		/// Private lobbies will not be displayed on the public list, only to those registered
		/// </summary>
		public bool IsPrivate { get; set; }

		/// <summary>
		/// The code used to directly join the lobby. Currently not implemented
		/// </summary>
		public string LobbyCode { get; set; }

		/// <summary>
		/// GUID of the lobby creator
		/// </summary>
		public string Owner { get; set; }

		/// <summary>
		/// A list of players registered to the game, either joined to public or given the GUID to join private
		/// </summary>
		public List<string> Players { get; set; }


		//NOTE: If JSONLobbyDataOptions.UseMultipleFiles is enabled, ignore this from the main lobby file
		//[JsonIgnore] Handle as one file for now
		/// <summary>
		/// Game settings to be passed to the engine
		/// </summary>
		public GameOptions GameOptions { get; set; }
		
		#endregion

		#region Dynamic Fields

		[JsonIgnore] public InnoTrainsEngine? GameEngine { get; set; }

		[JsonIgnore] public INetworkEngine? NetworkEngine { get; set; }

		[JsonIgnore] public LobbyState State { get; set; } = LobbyState.Closed;

		#endregion
	}
}
