namespace InnoTrains.Models.Data
{
	public class JSONFileOptions
	{
		public string Extension { get; } = "json";
		public required string PrefixPath { get; set; }
		public string SuffixPath { get; set; }
	}
}
