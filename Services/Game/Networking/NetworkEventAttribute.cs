using System.Reflection;

namespace InnoTrains.Services.Game.Networking
{
	/// <summary>
	/// Methods using this attribute should have exactly the parameters:
	/// (string lobbyGuid, string userGuid, string JsonData [Only if data is sent])
	/// 
	/// If used on a class, all methods in that class will append to that route
	/// Inherited false as the INetworkEngine should iterate through inheritance itself
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class NetworkEventAttribute : Attribute
	{
		public string Route { get; }
		public bool IgnoreClassAttribute { get; }

		/// <summary>
		/// Constructor for NetworkEventAttribute
		/// </summary>
		/// <param name="route">Route assigned to the method or class</param>
		/// <param name="ignoreClass">Whether to ignore the route assigned to the class, does nothing if targeting a class</param>
		public NetworkEventAttribute(string route, bool ignoreClass=false)
		{
			Route = route;
			IgnoreClassAttribute = ignoreClass;
		}
	}
}
