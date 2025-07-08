namespace InnoTrains.Models.Game
{
	/// <summary>
	/// Game options are options passed to the game engine on game start, on changing these
	/// the engine should be restarted
	/// </summary>
	public class GameOptions
	{
		public enum EmptyAction
		{
			None,
			Pause,
			Shutdown
		}

		/// <summary>
		/// Whether the server should pause or shutdown when there are no players connected
		/// </summary>
		public EmptyAction LobbyEmptyAction { get; set; } = EmptyAction.Shutdown;
	};
}
