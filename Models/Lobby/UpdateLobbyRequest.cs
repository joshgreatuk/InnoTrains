using InnoTrains.Models.Game;

namespace InnoTrains.Models.Lobby;

public class UpdateLobbyRequest
{
    public string LobbyGuid { get; set; }
    public GameOptions GameOptions { get; set; }
    public bool IsPrivate { get; set; }
}