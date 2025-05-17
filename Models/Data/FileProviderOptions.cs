namespace InnoTrains.Models.Data
{
	public class FileProviderOptions
	{
		public required string Extension { get; set; }
		public required string PrefixPath { get; set; }
		public string SuffixPath { get; set; }
	}
}
