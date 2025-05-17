using InnoTrains.Endpoints;
using InnoTrains.Models.Game.Networking;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices.ObjectiveC;
using System.Text.Json;
using System.Xml.Linq;

namespace InnoTrains.Services.Game.Networking
{
	public class WebsocketEngine : INetworkEngine
	{
		private readonly WebsocketEngineOptions Options;

		public Dictionary<string, Action<string>> websocketClients = new();

		//networkEvents listen for client interactions / internal server events
		public Dictionary<string, List<NetworkEventInfo>> networkEvents = new();

		//WebsocketEvent contains the event data in a JSON string and the message metadata
		public Queue<WebsocketMessage> interactionQueue = new();
		public Queue<WebsocketMessage> eventQueue = new();

		public WebsocketEngine(WebsocketEngineOptions options)
		{
			Options = options;
		}

		#region Game Engine Methods
		public void Start(IServiceProvider services)
		{
			//Add all network events from game services
			foreach (BaseGameService gameService in services.GetServices<BaseGameService>())
			{
				RegisterNetworkEvents(gameService);
			}
		}

		public void InteractionsUpdate(double delta)
		{
			while (interactionQueue.Count > 0)
			{
				WebsocketMessage message = interactionQueue.Dequeue();
				RecieveInteraction(message);
			}
		}

		public void EventsUpdate(double delta)
		{
			while (eventQueue.Count > 0)
			{
				WebsocketMessage message = eventQueue.Dequeue();
				SendEvent(message);
			}
		}

		public void Exit(IServiceProvider services)
		{
			//Remove all network events from game services
			foreach (BaseGameService gameService in services.GetServices<BaseGameService>())
			{
				UnregisterNetworkEvents(gameService);
			}
		}
		#endregion

		#region Event / Interaction Registration
		/// <summary>
		/// Send an event to a specific client
		/// </summary>
		/// <param name="route"></param>
		/// <param name="recieverId"></param>
		/// <param name="rawMessage"></param>
		public void SendMessage(string route, string recieverId, object rawMessage, bool isSuccess=true)
		{
			//Queue an event to specific user
			WebsocketMessage message = new(route, recieverId, SerializeEventData(rawMessage), isSuccess);

			if (Options.RouteQueuePrefixes.Any(x => message.Route.StartsWith(x)))
			{
				//Queue
				eventQueue.Enqueue(message);
				return;
			}

			SendEvent(message);
		}

		/// <summary>
		/// Send an event to a specific list of clients
		/// </summary>
		/// <param name="route"></param>
		/// <param name="recieverIds"></param>
		/// <param name="rawMessage"></param>
		public void SendMessage(string route, string[] recieverIds, object rawMessage)
		{
			//Queue multiple events to specific users
			for (int i = 0; i < recieverIds.Length; i++)
			{
				SendMessage(route, recieverIds[i], rawMessage);
			}
		}

		/// <summary>
		/// Send an event to every client
		/// </summary>
		/// <param name="route"></param>
		/// <param name="rawMessage"></param>
		public void BroadcastMessage(string route, object rawMessage)
		{
			//Queue multiple events to every user
			string[] clientIds = websocketClients.Keys.ToArray();
			for (int i = 0; i < websocketClients.Count; i++)
			{
				SendMessage(route, clientIds[i], rawMessage);
			}
		}

		/// <summary>
		/// Callback from the websocket controller for recieving a client interaction
		/// </summary>
		/// <param name="senderId"></param>
		/// <param name="rawMessage"></param>
		public void RecieveMessage(string senderId, string rawMessage)
		{
			//Remember to check senderId against metadata UserId, no need, just user senderId
			WebsocketMessage message = DeserializeMessage(rawMessage);
			message.UserId = senderId;
			
			if (Options.RouteQueuePrefixes.Any(x => message.Route.StartsWith(x)))
			{
				//Queue
				interactionQueue.Enqueue(message);
				return;
			}

			RecieveInteraction(message);
		}
		#endregion

		#region Event / Interaction Networking
		private void SendEvent(WebsocketMessage message)
		{
			if (!websocketClients.TryGetValue(message.UserId, out Action<string> sendAction))
			{
				//TODO: Write logging
				return;
			}

			//Send data to client
			sendAction.Invoke(SerializeMessage(message));
		}

		private void RecieveInteraction(WebsocketMessage message)
		{
			//TODO: Add preconditions/auto deserialization

			//Send event data to every networkEvent with this route
			if (!networkEvents.TryGetValue(message.Route, out List<NetworkEventInfo> routeEvents))
			{
				//TODO: Write Logging
				return;
			}

			for (int i = 0; i < routeEvents.Count; i++)
			{
				try
				{
					NetworkEventInfo info = routeEvents[i];
					info.Method.Invoke(info.Target, [message.UserId, message.Data]);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);

					//Send back an error event via the same route
					SendMessage(message.Route, message.UserId, ex.Message, false);
				}
			}
		}
		#endregion

		#region JSON Serialization
		/// <summary>
		/// A universal method to serialize event data classes into JSON
		/// </summary>
		/// <param name="rawMessage"></param>
		/// <returns></returns>
		private string SerializeEventData(object rawMessage)
		{
			return JsonSerializer.Serialize(rawMessage, new JsonSerializerOptions() { WriteIndented = true });
		}

		/// <summary>
		/// Universal method to serialize WebsocketEvents into JSON
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		private string SerializeMessage(WebsocketMessage message)
		{
			return JsonSerializer.Serialize(message, new JsonSerializerOptions() { WriteIndented = true });
		}

		/// <summary>
		/// Universal method to deserialize a WebsocketEvent from JSON
		/// </summary>
		/// <param name="rawMessage"></param>
		/// <returns></returns>
		private WebsocketMessage DeserializeMessage(string rawMessage)
		{
			return JsonSerializer.Deserialize<WebsocketMessage>(rawMessage, new JsonSerializerOptions() { WriteIndented = true });
		}
		#endregion

		#region Client Registration
		public void RegisterClient(Action<string> sendAction, string clientId)
		{
			//Check client isnt already registered, then register client and send event
			if (websocketClients.ContainsKey(clientId))
			{
				return;
			}

			websocketClients.Add(clientId, sendAction);
			BroadcastMessage("client_connected", clientId);
			RecieveInteraction(new WebsocketMessage("client_connected", "", clientId));
		}

		public void UnregisterClient(string clientId)
		{
			//Check client isnt already registered, then register client and send event
			if (!websocketClients.ContainsKey(clientId))
			{
				return;
			}

			websocketClients.Remove(clientId);
			BroadcastMessage("client_disconnected", clientId);
			RecieveInteraction(new WebsocketMessage("client_disconnected", "", clientId));
		}
		#endregion

		#region Event Registration
		public void RegisterNetworkEvents(object target)
		{
			List<NetworkEventInfo> events = GetObjectNetworkEvents(target);
			foreach (NetworkEventInfo eventInfo in events)
			{
				if (!networkEvents.TryGetValue(eventInfo.Route, out List<NetworkEventInfo> routeEvents)) 
				{
					routeEvents = new();
					networkEvents.Add(eventInfo.Route, routeEvents);
				}

				if (routeEvents.Any(x => x.Target == target && x.Method == eventInfo.Method))
				{
					continue;
				}

				routeEvents.Add(eventInfo);
			}
		}

		public void UnregisterNetworkEvents(object target)
		{
			List<NetworkEventInfo> events = GetObjectNetworkEvents(target);
			foreach (NetworkEventInfo eventInfo in events)
			{
				//If event is registered, unregister it
				if (!networkEvents.TryGetValue(eventInfo.Route, out List<NetworkEventInfo> routeEvents))
				{
					continue;
				}

				int eventCount = routeEvents.Count;
				for (int i = 0; i < eventCount;)
				{
					if (routeEvents[i].Target != target || routeEvents[i].Method != eventInfo.Method)
					{
						i++;
						continue;
					}

					routeEvents.RemoveAt(i);
					eventCount--;
				}
			}
		}

		private List<NetworkEventInfo> GetObjectNetworkEvents(object target)
		{
			//First construct the inheritance chain as a queue
			Queue<Type> typeQueue = new();
			typeQueue.Append(target.GetType());

			Type currentType = target.GetType();
			while (currentType.BaseType != null)
			{
				typeQueue.Enqueue(currentType.BaseType);
				currentType = currentType.BaseType;
			}

			typeQueue = new (typeQueue.Reverse());

			List<NetworkEventInfo> events = new();

			string currentRoute = "";
			while (typeQueue.Count > 0)
			{
				currentType = typeQueue.Dequeue();
				//Check the class for the attribute first
				NetworkEventAttribute attribute = currentType.GetCustomAttribute<NetworkEventAttribute>();
				if (attribute != null)
				{
					string appendRoute = attribute.Route;
					if (currentRoute == string.Empty && appendRoute.FirstOrDefault() == '/')
					{
						appendRoute.Remove(0, 1);
					}
					if (appendRoute.LastOrDefault() != '/')
					{
						appendRoute += '/';
					}
					currentRoute += attribute.Route;
				}

				//Check methods in the class for the attribute
				foreach (MethodInfo method in currentType.GetMethods())
				{
					attribute = currentType.GetCustomAttribute<NetworkEventAttribute>();
					if (attribute == null)
					{
						continue;
					}

					string appendRoute = attribute.Route;
					if (appendRoute.FirstOrDefault() == '/')
					{
						appendRoute.Remove(0, 1);
					}
					if (appendRoute.LastOrDefault() == '/')
					{
						appendRoute.Remove(appendRoute.Length - 1, 1);
					}

					NetworkEventInfo eventInfo = new NetworkEventInfo(target, method, currentRoute + appendRoute);
					events.Append(eventInfo);
				}
			}

			return events;
		}
		#endregion

		#region Init/Shutdown
		public void Shutdown(string shutdownMessage)
		{
			networkEvents.Clear();
			//Disconnect clients with shutdownMessage
			//TODO
		}
		#endregion
	}
}
