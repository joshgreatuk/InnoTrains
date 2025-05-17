using System.Reflection;

namespace InnoTrains.Models.Game.Networking
{
	public class NetworkEventInfo
	{
		public object Target { get; set; }
		public MethodInfo Method { get; set; }
		public string Route { get; set; }

		public NetworkEventInfo() { }
		public NetworkEventInfo(object target, MethodInfo method, string route)
		{
			Target = target;
			Method = method;
			Route = route;
		}
	}
}
