using ChatApp.Server.Models;
using ChatApp.Server.Services;
using Xunit;

namespace ChatApp.Tests;

public class MessageValidatorTests
{
    private readonly MessageValidator _validator = new();

    [Fact]
    public void Validate_ReturnsError_WhenMessageIsNull()
    {
        var result = _validator.Validate(null);

        Assert.False(result.IsValid);
        Assert.Equal("Message cannot be empty.", result.ErrorMessage);
    }

    [Fact]
    public void Validate_ReturnsError_WhenMessageTypeIsMissing()
    {
        var message = new ChatMessage
        {
            SenderId = "user-1",
            ReceiverId = "user-2",
            Data = "Hello"
        };

        var result = _validator.Validate(message);

        Assert.False(result.IsValid);
        Assert.Equal("Message type is required.", result.ErrorMessage);
    }

    [Fact]
    public void Validate_ReturnsSuccess_WhenTypingStopMessageIsValid()
    {
        var message = new ChatMessage
        {
            Type = "typing",
            SenderId = "user-1",
            ReceiverId = "user-2",
            Data = "stop"
        };

        var result = _validator.Validate(message);

        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void Validate_ReturnsError_WhenTypingStatusIsInvalid()
    {
        var message = new ChatMessage
        {
            Type = "typing",
            SenderId = "user-1",
            ReceiverId = "user-2",
            Data = "typing"
        };

        var result = _validator.Validate(message);

        Assert.False(result.IsValid);
        Assert.Equal("Typing status must be start or stop.", result.ErrorMessage);
    }

    [Fact]
    public void Validate_ReturnsError_WhenMessageTypeIsUnsupported()
    {
        var message = new ChatMessage
        {
            Type = "unknown",
            SenderId = "user-1",
            ReceiverId = "user-2",
            Data = "Hello"
        };

        var result = _validator.Validate(message);

        Assert.False(result.IsValid);
        Assert.Equal("Unsupported message type.", result.ErrorMessage);
    }

    [Fact]
    public void Validate_ReturnsError_WhenSenderIdIsMissing()
    {
        var message = new ChatMessage
        {
            Type = "chat",
            ReceiverId = "user-2",
            Data = "Hello"
        };

        var result = _validator.Validate(message);

        Assert.False(result.IsValid);
        Assert.Equal("Sender id is required.", result.ErrorMessage);
    }

    [Fact]
    public void Validate_ReturnsError_WhenChatReceiverIdIsMissing()
    {
        var message = new ChatMessage
        {
            Type = "chat",
            SenderId = "user-1",
            Data = "Hello"
        };

        var result = _validator.Validate(message);

        Assert.False(result.IsValid);
        Assert.Equal("Receiver id is required for chat messages.", result.ErrorMessage);
    }

    [Fact]
    public void Validate_ReturnsError_WhenChatTextIsMissing()
    {
        var message = new ChatMessage
        {
            Type = "chat",
            SenderId = "user-1",
            ReceiverId = "user-2"
        };

        var result = _validator.Validate(message);

        Assert.False(result.IsValid);
        Assert.Equal("Message text is required.", result.ErrorMessage);
    }

    [Fact]
    public void Validate_ReturnsError_WhenChatTextIsTooLong()
    {
        var message = new ChatMessage
        {
            Type = "chat",
            SenderId = "user-1",
            ReceiverId = "user-2",
            Data = new string('a', 501)
        };

        var result = _validator.Validate(message);

        Assert.False(result.IsValid);
        Assert.Equal("Message text cannot be longer than 500 characters.", result.ErrorMessage);
    }

    [Fact]
    public void Validate_ReturnsSuccess_WhenChatMessageIsValid()
    {
        var message = new ChatMessage
        {
            Type = "chat",
            SenderId = "user-1",
            ReceiverId = "user-2",
            Data = "Hello"
        };

        var result = _validator.Validate(message);

        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void Validate_ReturnsSuccess_WhenConnectMessageIsValid()
    {
        var message = new ChatMessage
        {
            Type = "connect",
            SenderId = "user-1"
        };

        var result = _validator.Validate(message);

        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void Validate_ReturnsSuccess_WhenTypingMessageIsValid()
    {
        var message = new ChatMessage
        {
            Type = "typing",
            SenderId = "user-1",
            ReceiverId = "user-2",
            Data = "start"
        };

        var result = _validator.Validate(message);

        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
    }
}
