using InnoTrains.Models.Data;

namespace InnoTrains.Services.Data
{
	public class JSONFileProvider
	{
		private JSONFileOptions LobbyConfig { get; } = new JSONFileOptions()
		{
			PrefixPath = "Games",
		};

		private JSONFileOptions GameConfig { get; } = new JSONFileOptions()
		{
			PrefixPath = "Games",
			SuffixPath = "Data"
		};

		private JSONFileOptions Config { get; }

		public JSONFileProvider(JSONFileOptions config)
		{
			Config = config;
		}

		/// <summary>
		/// File name does not include extension
		/// </summary>
		/// <param name="identifier"></param>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public bool DoesFileExist(string identifier, string fileName)
		{
			return File.Exists(GetFilePath(identifier, fileName));
		}

		/// <summary>
		/// File name does not include extension
		/// </summary>
		/// <param name="identifier"></param>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public string LoadJSON(string identifier, string fileName)
		{
			if (!DoesFileExist(identifier, fileName))
			{
				return "";
			}

			return File.ReadAllText(GetFilePath(identifier, fileName));
		}

		/// <summary>
		/// File names does not include extension
		/// </summary>
		/// <param name="identifier"></param>
		/// <param name="fileName"></param>
		/// <param name="data"></param>
		public string[] LoadJSON(string identifier, string[] fileNames)
		{
			return fileNames.Select(fileName => LoadJSON(identifier, fileName)).ToArray();
		}

		/// <summary>
		/// File names does not include extension
		/// </summary>
		/// <param name="identifier"></param>
		/// <param name="fileName"></param>
		/// <param name="data"></param>
		public string[] LoadJSON(string[] identifiers, string fileName)
		{
			return identifiers.Select(identifier => LoadJSON(identifier, fileName)).ToArray();
		}

		/// <summary>
		/// File name does not include extension
		/// </summary>
		/// <param name="identifier"></param>
		/// <param name="fileName"></param>
		/// <param name="data"></param>
		public void SaveJSON(string identifier, string fileName, string data)
		{
			string path = GetFilePath(identifier, fileName);

			string backupJSON = "";
			if (DoesFileExist(identifier, fileName))
			{
				backupJSON = LoadJSON(identifier, fileName);
			}

			try
			{
				File.WriteAllText(path, data);
			}
			catch (Exception ex)
			{
				//Restore backup
				if (backupJSON != String.Empty)
				{
					File.WriteAllText(path, backupJSON);
				}
				throw;
			}
		}

		/// <summary>
		/// File names does not include extension
		/// </summary>
		/// <param name="identifier"></param>
		/// <param name="fileName"></param>
		/// <param name="data"></param>
		public void SaveJSON(string identifiter, string[] fileNames, string[] data)
		{
			if (fileNames.Length != data.Length)
			{
				throw new ArgumentOutOfRangeException("fileNames.Length and data.Length is different");
			}

			for (int i=0; i < fileNames.Length; i++)
			{
				SaveJSON(identifiter, fileNames[i], data[i]);
			}
		}

		/// <summary>
		/// File names does not include extension
		/// </summary>
		/// <param name="identifier"></param>
		/// <param name="fileName"></param>
		/// <param name="data"></param>
		public void SaveJSON(string[] identifiers, string fileName, string[] data)
		{
			if (identifiers.Length != data.Length)
			{
				throw new ArgumentOutOfRangeException("identifiers.Length and data.Length is different");
			}

			for (int i = 0; i < identifiers.Length; i++)
			{
				SaveJSON(identifiers[i], fileName, data[i]);
			}
		}

		private string GetFilePath(string identifier, string fileName)
		{
			return GetDirectoryPath(identifier + $"/{fileName}.{Config.Extension}");
		}
		

		private string GetDirectoryPath(string identifier)
		{
			return Directory.GetCurrentDirectory() + $"/{Config.PrefixPath}/{identifier}/{Config.SuffixPath}";
		}
	}
}
