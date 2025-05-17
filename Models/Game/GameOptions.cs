namespace InnoTrains.Models.Game
{
	/// <summary>
	/// Game options are options passed to the game engine on game start, on changing these
	/// the engine should be restarted
	/// </summary>
	public class GameOptions
	{
		/// <summary>
		/// Should the lobby pause when empty
		/// </summary>
		public bool PauseWhenEmpty { get; set; }
	}
}
