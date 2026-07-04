namespace ChatApp.Server.Models;

public class ChatMessage
{
    public string Type { get; set; } = string.Empty;

    public string SenderId { get; set; } = string.Empty;

    public string ReceiverId { get; set; } = string.Empty;

    public string Data { get; set; } = string.Empty;

    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}
