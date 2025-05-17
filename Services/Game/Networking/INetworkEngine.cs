namespace InnoTrains.Services.Game.Networking
{
	/// <summary>
	/// The network engine is responsible for:
	/// - Tracking connected clients
	/// - Distribution of messages to the correct clients
	/// - Distribution of recieved messages to correct services (via attributes/observers)
	/// - Ensuring messages are only processed in tick update
	/// - (Possible) Multi-threading of message processing
	/// 
	/// The network engine runs independant of the game loop as certain events must
	/// be fired even when the game is paused or stopped, for example, in the lobby
	/// 
	/// The network controller will be held as a reference in both the lobby and game engine
	/// 
	/// Messages should be in JSON, the sender/reciever method is responsible for serialization
	/// </summary>
	public interface INetworkEngine
	{
		/// <summary>
		/// When the game is started, this lets the network controller know to accept game events
		/// This is where network event attribute scanning will happen, registering events under /game/
		/// </summary>
		public void Start(IServiceProvider services);

		/// <summary>
		/// Game update tick to handle user interactions (before BaseGameService updates) (Client -> Server)
		/// </summary>
		/// <param name="delta"></param>
		public void InteractionsUpdate(double delta);

		/// <summary>
		/// Game update tick to handle service events (after BaseGameService updates) (Server -> Client)
		/// </summary>
		/// <param name="delta"></param>
		public void EventsUpdate(double delta);

		/// <summary>
		/// Lets the network controller know when to stop accepting game events
		/// Ensure that routes that begin with /game/ are unregistered here
		/// </summary>
		public void Exit(IServiceProvider services);

		/// <summary>
		/// Shutdown the network controller, for example the last client has left or the lobby is deleted
		/// </summary>
		public void Shutdown(string shutdownMessage);

		/// <summary>
		/// Messages should always be in JSON format
		/// </summary>
		/// <param name="recieverId"></param>
		/// <param name="message"></param>
		public void SendMessage(string route, string recieverId, object message, bool isSuccess=true);
		/// <summary>
		/// Messages should always be in JSON format
		/// </summary>
		/// <param name="recieverIds"></param>
		/// <param name="message"></param>
		public void SendMessage(string route, string[] recieverIds, object message);

		/// <summary>
		/// Messages should always be in JSON format
		/// </summary>
		/// <param name="message"></param>
		public void BroadcastMessage(string route, object message);

		/// <summary>
		/// Messages should always be in JSON format
		/// </summary>
		/// <param name="senderId"></param>
		/// <param name="message"></param>
		public void RecieveMessage(string senderId, string message);

		public void RegisterClient(Action<string> sendAction, string clientId);
		public void UnregisterClient(string clientId);

		public void RegisterNetworkEvents(object target);
		public void UnregisterNetworkEvents(object target);
	}
}
