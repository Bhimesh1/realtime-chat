using ChatApp.Server.Models;

namespace ChatApp.Server.Services;

public class MessageStore
{
    private const int MaxMessagesPerConversation = 100;

    private readonly Dictionary<string, List<ChatMessage>> _messagesByConversation = new();
    private readonly object _lock = new();

    public void Add(ChatMessage message)
    {
        var key = GetConversationKey(message.SenderId, message.ReceiverId);

        lock (_lock)
        {
            if (!_messagesByConversation.TryGetValue(key, out var messages))
            {
                messages = new List<ChatMessage>();
                _messagesByConversation[key] = messages;
            }

            messages.Add(CloneMessage(message));

            if (messages.Count > MaxMessagesPerConversation)
            {
                messages.RemoveAt(0);
            }
        }
    }

    public IReadOnlyList<ChatMessage> GetConversation(string firstUserId, string secondUserId)
    {
        var key = GetConversationKey(firstUserId, secondUserId);

        lock (_lock)
        {
            if (!_messagesByConversation.TryGetValue(key, out var messages))
            {
                return Array.Empty<ChatMessage>();
            }

            return messages
                .OrderBy(message => message.SentAt)
                .Select(CloneMessage)
                .ToList();
        }
    }

    private static string GetConversationKey(string firstUserId, string secondUserId)
    {
        var users = new[] { firstUserId.Trim(), secondUserId.Trim() }
            .Order(StringComparer.OrdinalIgnoreCase);

        return string.Join(":", users).ToLowerInvariant();
    }

    private static ChatMessage CloneMessage(ChatMessage message)
    {
        return new ChatMessage
        {
            Type = message.Type,
            SenderId = message.SenderId,
            ReceiverId = message.ReceiverId,
            Data = message.Data,
            SentAt = message.SentAt
        };
    }
}
