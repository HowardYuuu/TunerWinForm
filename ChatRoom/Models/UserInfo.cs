namespace ChatRoom.Models;

public class UserInfo
{
    public string ConnectionId { get; set; } = string.Empty;
    public string Nickname { get; set; } = string.Empty;
    public DateTime ConnectedAt { get; set; }
}
