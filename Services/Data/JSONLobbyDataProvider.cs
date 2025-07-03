using InnoTrains.Models.Lobby;
using System.IO;

namespace InnoTrains.Services.Data
{
	public class JSONLobbyDataProvider : ILobbyDataProvider
	{
		public static FileProvider LobbyFileProvider { get; private set; }

		public JSONLobbyDataProvider()
		{
			LobbyFileProvider = new FileProvider(FileProvider.LobbyConfig);
		}

		public string[] GetPublicLobbies()
		{
			List<string> lobbies = new();
			foreach (string dirName in Directory.EnumerateDirectories(LobbyFileProvider.GetDirectoryPath("")))
			{
				if (!dirName.StartsWith("public_"))
				{
					continue;
				}
				lobbies.Add(dirName.Replace("." + LobbyFileProvider.Config.Extension, ""));
			}
			return lobbies.ToArray();
		}

		public LobbyInfo LoadLobby(string guid, bool loadOptions)
		{
			//Grab lobby info
			throw new NotImplementedException();
		}

		public void SaveLobby(LobbyInfo lobby)
		{
			throw new NotImplementedException();
		}

		public LobbyInfo GetLobbyByGuid(string guid)
		{
			throw new NotImplementedException();
		}

		public LobbyInfo DeleteLobby(string guid)
		{
			throw new NotImplementedException();
		}
	}
}
