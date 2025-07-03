using InnoTrains.Models.Lobby;

namespace InnoTrains.Services.Data
{
	/// <summary>
	/// NOTE: Public lobby list should be cached, likely in a static property. For now it refreshes every time.
	/// NOTE: It is the responsibility of the lobby service to check user permissions
	/// </summary>
	public interface ILobbyDataProvider
	{
		public void SaveLobby(LobbyInfo lobby);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="guid"></param>
		/// <param name="loadOptions">Whether to load game options, if we are just checking the file, we don't need loadOptions</param>
		/// <returns></returns>
		public LobbyInfo LoadLobby(string guid, bool loadOptions);

		public LobbyInfo DeleteLobby(string guid);

		public string[] GetPublicLobbies();

		public LobbyInfo GetLobbyByGuid(string guid);
	}
}
