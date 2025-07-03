using InnoTrains.Models.Data;

namespace InnoTrains.Services.Data
{
	public class FileProvider
	{
		public static FileProviderOptions LobbyConfig { get; } = new FileProviderOptions()
		{
			Extension = "json",
			PrefixPath = "Games",
		};

		public static FileProviderOptions GameConfig { get; } = new FileProviderOptions()
		{
			Extension = "json",
			PrefixPath = "Games",
			SuffixPath = "Data"
		};

		public FileProviderOptions Config { get; }

		public FileProvider(FileProviderOptions config)
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
		public string LoadFile(string identifier, string fileName)
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
		public string[] LoadFile(string identifier, string[] fileNames)
		{
			return fileNames.Select(fileName => LoadFile(identifier, fileName)).ToArray();
		}

		/// <summary>
		/// File names does not include extension
		/// </summary>
		/// <param name="identifier"></param>
		/// <param name="fileName"></param>
		/// <param name="data"></param>
		public string[] LoadFile(string[] identifiers, string fileName)
		{
			return identifiers.Select(identifier => LoadFile(identifier, fileName)).ToArray();
		}

		/// <summary>
		/// File name does not include extension
		/// </summary>
		/// <param name="identifier"></param>
		/// <param name="fileName"></param>
		/// <param name="data"></param>
		public void SaveFile(string identifier, string fileName, string data)
		{
			string path = GetFilePath(identifier, fileName);

			string backupJSON = "";
			if (DoesFileExist(identifier, fileName))
			{
				backupJSON = LoadFile(identifier, fileName);
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
		public void SaveFile(string identifiter, string[] fileNames, string[] data)
		{
			if (fileNames.Length != data.Length)
			{
				throw new ArgumentOutOfRangeException("fileNames.Length and data.Length is different");
			}

			for (int i=0; i < fileNames.Length; i++)
			{
				SaveFile(identifiter, fileNames[i], data[i]);
			}
		}

		/// <summary>
		/// File names does not include extension
		/// </summary>
		/// <param name="identifier"></param>
		/// <param name="fileName"></param>
		/// <param name="data"></param>
		public void SaveFile(string[] identifiers, string fileName, string[] data)
		{
			if (identifiers.Length != data.Length)
			{
				throw new ArgumentOutOfRangeException("identifiers.Length and data.Length is different");
			}

			for (int i = 0; i < identifiers.Length; i++)
			{
				SaveFile(identifiers[i], fileName, data[i]);
			}
		}

		/// <summary>
		/// File names should not include extension
		/// </summary>
		public void DeleteFile(string identifier, string fileName)
		{
			string filePath = GetFilePath(identifier, fileName);
			if (!File.Exists(filePath))
			{
				return;
			}

			File.Delete(filePath);
		}

		public void DeleteFolder(string identifier)
		{
			string dirPath = GetDirectoryPath(identifier);
			if (!Directory.Exists(dirPath))
			{
				return;
			}

			Directory.Delete(dirPath, true);
		}

		public string GetFilePath(string identifier, string fileName)
		{
			return GetDirectoryPath(identifier + $"/{fileName}.{Config.Extension}");
		}
		

		public string GetDirectoryPath(string identifier)
		{
			return 
				Directory.GetCurrentDirectory() + 
				$"/" +
				$"{(Config.PrefixPath != null ? Config.PrefixPath + "/" : "")}" +
				$"{(identifier != string.Empty ? identifier + "/" : "")}" +
				$"{(Config.SuffixPath != string.Empty && identifier != string.Empty ? Config.SuffixPath + "/" : "")}";
		}
	}
}
