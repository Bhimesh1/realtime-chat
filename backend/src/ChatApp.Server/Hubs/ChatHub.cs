using System.Collections.Concurrent;
using ChatApp.Server.Models;
using ChatApp.Server.Services;
using Microsoft.AspNetCore.SignalR;

namespace ChatApp.Server.Hubs;

public class ChatHub : Hub
{
    private static readonly ConcurrentDictionary<string, string> ConnectedUsers = new(
        StringComparer.OrdinalIgnoreCase);

    private readonly MessageValidator _messageValidator;
    private readonly MessageStore _messageStore;

    public ChatHub(MessageValidator messageValidator, MessageStore messageStore)
{
    _messageValidator = messageValidator;
    _messageStore = messageStore;
}

    public async Task RegisterUser(ChatMessage message)
    {
        var validationResult = _messageValidator.Validate(message);

        if (!validationResult.IsValid)
        {
            await SendError(validationResult.ErrorMessage!);
            return;
        }

        if (!message.Type.Equals("connect", StringComparison.OrdinalIgnoreCase))
        {
            await SendError("Only connect messages can register a user.");
            return;
        }

        var senderId = message.SenderId.Trim();

        RemoveExistingUserForCurrentConnection();

        ConnectedUsers[senderId] = Context.ConnectionId;

        await Clients.All.SendAsync("UserPresenceChanged", senderId, true);
        await Clients.Caller.SendAsync("OnlineUsers", ConnectedUsers.Keys.Order().ToList());
    }

    public async Task SendMessage(ChatMessage message)
    {
        var validationResult = _messageValidator.Validate(message);

        if (!validationResult.IsValid)
        {
            await SendError(validationResult.ErrorMessage!);
            return;
        }

        if (!message.Type.Equals("chat", StringComparison.OrdinalIgnoreCase))
        {
            await SendError("Only chat messages can be sent through SendMessage.");
            return;
        }

        if (!IsRegisteredSender(message.SenderId))
        {
            await SendError("Sender is not registered on this connection.");
            return;
        }

        message.SentAt = DateTime.UtcNow;

        if (!ConnectedUsers.TryGetValue(message.ReceiverId, out var receiverConnectionId))
        {
            await SendError("Receiver is currently offline.");
            return;
        }

        _messageStore.Add(message);

        await Clients.Client(receiverConnectionId).SendAsync("ReceiveMessage", message);
        await Clients.Caller.SendAsync("ReceiveMessage", message);
    }

    public Task<IReadOnlyList<ChatMessage>> GetHistory(string receiverId)
    {
        if (string.IsNullOrWhiteSpace(receiverId))
        {
            return Task.FromResult<IReadOnlyList<ChatMessage>>(Array.Empty<ChatMessage>());
        }

        var senderId = GetCurrentRegisteredUserId();

        if (string.IsNullOrWhiteSpace(senderId))
        {
            return Task.FromResult<IReadOnlyList<ChatMessage>>(Array.Empty<ChatMessage>());
        }

        if (senderId.Equals(receiverId, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<IReadOnlyList<ChatMessage>>(Array.Empty<ChatMessage>());
        }

        var history = _messageStore.GetConversation(senderId, receiverId);

        return Task.FromResult(history);
    }

    public async Task SendTyping(ChatMessage message)
    {
        var validationResult = _messageValidator.Validate(message);

        if (!validationResult.IsValid)
        {
            await SendError(validationResult.ErrorMessage!);
            return;
        }

        if (!message.Type.Equals("typing", StringComparison.OrdinalIgnoreCase))
        {
            await SendError("Only typing messages can be sent through SendTyping.");
            return;
        }

        if (!IsRegisteredSender(message.SenderId))
        {
            await SendError("Sender is not registered on this connection.");
            return;
        }

        if (ConnectedUsers.TryGetValue(message.ReceiverId, out var receiverConnectionId))
        {
            await Clients.Client(receiverConnectionId).SendAsync("ReceiveTyping", message);
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var disconnectedUser = ConnectedUsers.FirstOrDefault(user =>
            user.Value == Context.ConnectionId);

        if (!string.IsNullOrWhiteSpace(disconnectedUser.Key))
        {
            ConnectedUsers.TryRemove(disconnectedUser.Key, out _);
            await Clients.All.SendAsync("UserPresenceChanged", disconnectedUser.Key, false);
        }

        await base.OnDisconnectedAsync(exception);
    }

    private bool IsRegisteredSender(string senderId)
    {
        return ConnectedUsers.TryGetValue(senderId, out var connectionId)
            && connectionId == Context.ConnectionId;
    }

    private void RemoveExistingUserForCurrentConnection()
    {
        var existingUser = ConnectedUsers.FirstOrDefault(user =>
            user.Value == Context.ConnectionId);

        if (!string.IsNullOrWhiteSpace(existingUser.Key))
        {
            ConnectedUsers.TryRemove(existingUser.Key, out _);
        }
    }

    private string GetCurrentRegisteredUserId()
    {
        var registeredUser = ConnectedUsers.FirstOrDefault(user =>
            user.Value == Context.ConnectionId);

        return registeredUser.Key ?? string.Empty;
    }

    private Task SendError(string errorMessage)
    {
        var error = new ChatMessage
        {
            Type = "error",
            SenderId = "server",
            ReceiverId = Context.ConnectionId,
            Data = errorMessage,
            SentAt = DateTime.UtcNow
        };

        return Clients.Caller.SendAsync("ReceiveError", error);
    }
}
