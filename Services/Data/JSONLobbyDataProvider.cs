using InnoTrains.Models.Data;
using InnoTrains.Models.Lobby;
using Microsoft.IdentityModel.Tokens;
using System.IO;
using System.Text.Json;

namespace InnoTrains.Services.Data
{
	public class JSONLobbyDataProvider : ILobbyDataProvider, IInitializable
	{
		private readonly JSONLobbyDataOptions Options;

		public static FileProvider LobbyFileProvider { get; private set; }

		private Dictionary<string, LobbyInfo> lobbies = new();

		public JSONLobbyDataProvider(JSONLobbyDataOptions options)
		{
			LobbyFileProvider = new FileProvider(FileProvider.LobbyConfig);
			Options = options;
		}

		public void Initialize()
		{
			//Load lobbies
			foreach (string folderName in Directory.GetDirectories(Directory.GetCurrentDirectory() + "/Games/"))
			{
				string lobbyJSON = LobbyFileProvider.LoadFile(folderName, "lobby");
				LobbyInfo? lobby = JsonSerializer.Deserialize<LobbyInfo>(lobbyJSON);
				if (lobby == null)
				{
					Console.WriteLine($"Lobby '{folderName}' failed to deserialize");
					continue;
				}

				lobbies.Add(lobby.GUID, lobby);
			}
		}

		public LobbyInfo[] GetPublicLobbies()
		{
			return lobbies.Where(x => !x.Value.IsPrivate).Select(x => x.Value).ToArray();

			///For when lobbies must be stored on disk instead of in a dict
			//List<string> lobbies = new();
			//foreach (string dirName in Directory.EnumerateDirectories(LobbyFileProvider.GetDirectoryPath("")))
			//{
			//	if (!dirName.StartsWith("public_"))
			//	{
			//		continue;
			//	}
			//	lobbies.Add(dirName.Replace("." + LobbyFileProvider.Config.Extension, ""));
			//}
			//return lobbies.ToArray();
		}

		public LobbyInfo LoadLobby(string guid, bool loadOptions)
		{
			//Grab lobby info
			return lobbies.Where(x => x.Key == guid).FirstOrDefault().Value;
		}

		public void SaveLobby(LobbyInfo lobby)
		{
			lobbies[lobby.GUID] = lobby;

			//Create JSON
			JsonSerializerOptions jsonOptions = new() { WriteIndented = true };
			string jsonData = JsonSerializer.Serialize(lobby, typeof(LobbyInfo), jsonOptions);

			LobbyFileProvider.SaveFile(lobby.GUID, "lobby", jsonData);
		}

		public void DeleteLobby(string guid)
		{
			//Remove lobby from provider
			if (lobbies.ContainsKey(guid))
			{
				lobbies.Remove(guid);
			}

			//Remove the JSON file, FileProvider has file existance protection
			LobbyFileProvider.DeleteFile(guid, "lobby");
		}
	}
}
