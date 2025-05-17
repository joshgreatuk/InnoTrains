using InnoTrains.Services.Game.Networking;

namespace InnoTrains.Services.Game
{
	[NetworkEvent("game")]
	public abstract class BaseGameService
	{
		protected readonly INetworkEngine NetworkEngine;

		public BaseGameService(INetworkEngine networkEngine)
		{
			NetworkEngine = networkEngine;
		}

		public virtual void Init() { }

		public virtual void Start() { }

		public virtual void Update(double delta) { }

		public virtual void Pause() { }

		public virtual void Unpause() { }

		public virtual string SaveJSON() => "";

		public virtual void Exit() { }

		public void SendEvent(string route, string reciever, object message)
		{
			NetworkEngine.SendMessage(route, reciever, message);
		}

		public void SendEvent(string route, string[] recievers, object message)
		{
			NetworkEngine.SendMessage(route, recievers, message);
		}

		public void BroadcastEvent(string route, object message)
		{ 
			NetworkEngine.BroadcastMessage(route, message);
		}
	}
}
