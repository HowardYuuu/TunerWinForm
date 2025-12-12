namespace ChatRoom.Models;

public class ChatMessage
{
    public string MessageId { get; set; } = Guid.NewGuid().ToString();
    public string Sender { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public MessageType Type { get; set; } = MessageType.Text;
    public string? ImageData { get; set; }
    public HashSet<string> ReadBy { get; set; } = new();
}

public enum MessageType
{
    Text,
    Image
}
