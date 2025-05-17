namespace InnoTrains.Models.Game.Networking
{
	public class WebsocketMessage
	{
		public string Route { get; set; }
		public string UserId { get; set; }
		public string Data { get; set; }
		public bool IsSuccess { get; set; }

		public WebsocketMessage () { }
		public WebsocketMessage (string userId, string route, string data, bool isSuccess=true)
		{
			Route = route;
			UserId = userId;
			Data = data;
			IsSuccess = isSuccess;
		}
	}
}
