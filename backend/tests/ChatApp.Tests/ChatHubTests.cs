using System.Security.Claims;
using ChatApp.Server.Hubs;
using ChatApp.Server.Models;
using ChatApp.Server.Services;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SignalR;

namespace ChatApp.Tests;

public class ChatHubTests
{
    [Fact]
    public async Task SendMessage_SendsError_WhenMessageIsInvalid()
    {
        var caller = new RecordingClientProxy();

        var hub = new ChatHub(new MessageValidator(), new MessageStore())
        {
            Context = new TestHubCallerContext("connection-1"),
            Clients = new TestHubCallerClients(caller)
        };

        await hub.SendMessage(null!);

        var call = Assert.Single(caller.Calls);

        Assert.Equal("ReceiveError", call.Method);

        var error = Assert.IsType<ChatMessage>(Assert.Single(call.Arguments));

        Assert.Equal("error", error.Type);
        Assert.Equal("server", error.SenderId);
        Assert.Equal("connection-1", error.ReceiverId);
        Assert.Equal("Message cannot be empty.", error.Data);
    }

    private sealed class RecordingClientProxy : IClientProxy
    {
        public List<(string Method, object?[] Arguments)> Calls { get; } = new();

        public Task SendCoreAsync(
            string method,
            object?[] args,
            CancellationToken cancellationToken = default)
        {
            Calls.Add((method, args));
            return Task.CompletedTask;
        }
    }

    private sealed class TestHubCallerClients : IHubCallerClients
    {
        private readonly IClientProxy _caller;
        private readonly IClientProxy _otherClients = new RecordingClientProxy();

        public TestHubCallerClients(IClientProxy caller)
        {
            _caller = caller;
        }

        public IClientProxy All => _otherClients;

        public IClientProxy Caller => _caller;

        public IClientProxy Others => _otherClients;

        public IClientProxy AllExcept(IReadOnlyList<string> excludedConnectionIds)
        {
            return _otherClients;
        }

        public IClientProxy Client(string connectionId)
        {
            return _otherClients;
        }

        public IClientProxy Clients(IReadOnlyList<string> connectionIds)
        {
            return _otherClients;
        }

        public IClientProxy Group(string groupName)
        {
            return _otherClients;
        }

        public IClientProxy GroupExcept(string groupName, IReadOnlyList<string> excludedConnectionIds)
        {
            return _otherClients;
        }

        public IClientProxy Groups(IReadOnlyList<string> groupNames)
        {
            return _otherClients;
        }

        public IClientProxy OthersInGroup(string groupName)
        {
            return _otherClients;
        }

        public IClientProxy User(string userId)
        {
            return _otherClients;
        }

        public IClientProxy Users(IReadOnlyList<string> userIds)
        {
            return _otherClients;
        }
    }

    private sealed class TestHubCallerContext : HubCallerContext
    {
        public TestHubCallerContext(string connectionId)
        {
            ConnectionId = connectionId;
        }

        public override string ConnectionId { get; }

        public override string? UserIdentifier => null;

        public override ClaimsPrincipal? User => null;

        public override IDictionary<object, object?> Items { get; } =
            new Dictionary<object, object?>();

        public override IFeatureCollection Features { get; } =
            new FeatureCollection();

        public override CancellationToken ConnectionAborted { get; } =
            CancellationToken.None;

        public override void Abort()
        {
        }
    }
}
