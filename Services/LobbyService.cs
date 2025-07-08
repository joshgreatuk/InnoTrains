using InnoTrains.Services.Data;

namespace InnoTrains.Services
{
	public class LobbyService
	{
		private readonly ILobbyDataProvider LobbyDataProvider;

		public LobbyService(ILobbyDataProvider lobbyDataProvider)
		{
			LobbyDataProvider = lobbyDataProvider;
		}
	}
}
