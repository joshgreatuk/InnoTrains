using InnoTrains.Models.Game;

namespace InnoTrains.Models.Lobby;

public class CreateLobbyRequest
{
    public string LobbyName { get; set; }
    public bool IsPrivate { get; set; }
    public GameOptions GameOptions { get; set; }
}