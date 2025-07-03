using InnoTrains.Models.Game;
using System.Text.Json.Serialization;

namespace InnoTrains.Models.Lobby
{
	/// <summary>
	/// LobbyInfo contains lobby information and settings such as permissions, capacities, etc
	/// </summary>
	public class LobbyInfo
	{
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
		//public string LobbyCode { get; set; }

		/// <summary>
		/// GUID of the lobby creator
		/// </summary>
		public string Owner { get; set; }

		/// <summary>
		/// A list of players registered to the game, either joined to public or given the GUID to join private
		/// </summary>
		public List<string> Players { get; set; }


		//NOTE: If JSONLobbyDataOptions.UseMultipleFiles is enabled, ignore this from the main lobby file
		[JsonIgnore]
		/// <summary>
		/// Game settings to be passed to the engine
		/// </summary>
		public GameOptions GameOptions { get; set; }
	}
}
