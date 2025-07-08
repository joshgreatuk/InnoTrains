using InnoTrains.Models.Data;
using InnoTrains.Models.Lobby;
using Microsoft.IdentityModel.Tokens;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace InnoTrains.Services.Data
{
	public class JsonLobbyDataProvider(JsonSerializerOptions serializerOptions, IOptions<JSONLobbyDataOptions> options) : ILobbyDataProvider, IInitializable
	{
		private readonly JsonSerializerOptions _serializerOptions = serializerOptions;
		private readonly JSONLobbyDataOptions _options = options.Value;

		public static FileProvider LobbyFileProvider { get; private set; } = new FileProvider(FileProvider.LobbyConfig);

		private Dictionary<string, LobbyInfo> _lobbies = new();
		
		public void Initialize()
		{
			//Load lobbies
			foreach (string folderName in Directory.GetDirectories(Directory.GetCurrentDirectory() + "/Games/"))
			{
				string lobbyJson = LobbyFileProvider.LoadFile(folderName, "lobby");
				LobbyInfo? lobby = JsonSerializer.Deserialize<LobbyInfo>(lobbyJson, _serializerOptions);
				if (lobby == null)
				{
					Console.WriteLine($"Lobby '{folderName}' failed to deserialize");
					continue;
				}

				_lobbies.Add(lobby.GUID, lobby);
			}
		}

		public LobbyInfo[] GetPublicLobbies()
		{
			return _lobbies.Where(x => !x.Value.IsPrivate).Select(x => x.Value).ToArray();

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

		public LobbyInfo? LoadLobby(string guid, bool loadOptions)
		{
			//Grab lobby info
			return _lobbies.Where(x => x.Key == guid).FirstOrDefault().Value;
		}

		public IEnumerable<LobbyInfo> GetLobbyEnumerable()
		{
			return _lobbies.Values.AsEnumerable();
		}

		public void SaveLobby(LobbyInfo lobby)
		{
			_lobbies[lobby.GUID] = lobby;

			//Create JSON
			string jsonData = JsonSerializer.Serialize(lobby, typeof(LobbyInfo), _serializerOptions);

			LobbyFileProvider.SaveFile(lobby.GUID, "lobby", jsonData);
		}

		public void DeleteLobby(string guid)
		{
			//Remove lobby from provider
			if (_lobbies.ContainsKey(guid))
			{
				_lobbies.Remove(guid);
			}

			//Remove the JSON file, FileProvider has file existance protection
			LobbyFileProvider.DeleteFile(guid, "lobby");
		}
	}
}
