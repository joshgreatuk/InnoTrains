using InnoTrains.Models.Game;

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
		/// The code used to directly join the lobby
		/// </summary>
		public string LobbyCode { get; set; }

		/// <summary>
		/// GUID of the lobby creator
		/// </summary>
		public string Owner { get; set; }

		/// <summary>
		/// Game settings to be passed to the engine
		/// </summary>
		public GameOptions GameOptions { get; set; }
	}
}
