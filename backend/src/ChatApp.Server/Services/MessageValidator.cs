using ChatApp.Server.Models;

namespace ChatApp.Server.Services;

public class MessageValidator
{
    private static readonly HashSet<string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "connect",
        "chat",
        "typing",
        "error"
    };

    public ValidationResult Validate(ChatMessage? message)
    {
        if (message is null)
        {
            return ValidationResult.Fail("Message cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(message.Type))
        {
            return ValidationResult.Fail("Message type is required.");
        }

        if (!AllowedTypes.Contains(message.Type))
        {
            return ValidationResult.Fail("Unsupported message type.");
        }

        if (string.IsNullOrWhiteSpace(message.SenderId))
        {
            return ValidationResult.Fail("Sender id is required.");
        }

        if (message.Type.Equals("chat", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(message.ReceiverId))
            {
                return ValidationResult.Fail("Receiver id is required for chat messages.");
            }

            if (string.IsNullOrWhiteSpace(message.Data))
            {
                return ValidationResult.Fail("Message text is required.");
            }

            if (message.Data.Length > 500)
            {
                return ValidationResult.Fail("Message text cannot be longer than 500 characters.");
            }
        }

        if (message.Type.Equals("typing", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(message.ReceiverId))
            {
                return ValidationResult.Fail("Receiver id is required for typing messages.");
            }
        }

        return ValidationResult.Success();
    }
}

public class ValidationResult
{
    public bool IsValid { get; }

    public string? ErrorMessage { get; }

    private ValidationResult(bool isValid, string? errorMessage)
    {
        IsValid = isValid;
        ErrorMessage = errorMessage;
    }

    public static ValidationResult Success()
    {
        return new ValidationResult(true, null);
    }

    public static ValidationResult Fail(string errorMessage)
    {
        return new ValidationResult(false, errorMessage);
    }
}
