using ChatApp.Server.Models;

namespace ChatApp.Server.Services;

public class MessageValidator
{
    private const int MaxUserIdLength = 64;
    private const int MaxChatMessageLength = 2000;

    private static readonly HashSet<string> AllowedClientTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "connect",
        "chat",
        "typing"
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

        if (!AllowedClientTypes.Contains(message.Type))
        {
            return ValidationResult.Fail("Unsupported message type.");
        }

        var senderValidationResult = ValidateUserId(message.SenderId, "Sender id");

        if (!senderValidationResult.IsValid)
        {
            return senderValidationResult;
        }

        if (message.Type.Equals("chat", StringComparison.OrdinalIgnoreCase))
        {
            var receiverValidationResult = ValidateReceiverId(message.ReceiverId);

            if (!receiverValidationResult.IsValid)
            {
                return receiverValidationResult;
            }

            if (message.SenderId.Trim().Equals(message.ReceiverId.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return ValidationResult.Fail("Sender and receiver cannot be the same user.");
            }

            if (string.IsNullOrWhiteSpace(message.Data))
            {
                return ValidationResult.Fail("Message text is required.");
            }

            if (message.Data.Length > MaxChatMessageLength)
            {
                return ValidationResult.Fail($"Message text cannot be longer than {MaxChatMessageLength} characters.");
            }
        }

        if (message.Type.Equals("typing", StringComparison.OrdinalIgnoreCase))
        {
            var receiverValidationResult = ValidateReceiverId(message.ReceiverId);

            if (!receiverValidationResult.IsValid)
            {
                return receiverValidationResult;
            }

            if (message.SenderId.Trim().Equals(message.ReceiverId.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return ValidationResult.Fail("Sender and receiver cannot be the same user.");
            }

            if (!message.Data.Equals("start", StringComparison.OrdinalIgnoreCase) &&
                !message.Data.Equals("stop", StringComparison.OrdinalIgnoreCase))
            {
                return ValidationResult.Fail("Typing status must be start or stop.");
            }
        }

        return ValidationResult.Success();
    }

    private static ValidationResult ValidateReceiverId(string receiverId)
    {
        if (string.IsNullOrWhiteSpace(receiverId))
        {
            return ValidationResult.Fail("Receiver id is required.");
        }

        return ValidateUserId(receiverId, "Receiver id");
    }

    private static ValidationResult ValidateUserId(string userId, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return ValidationResult.Fail($"{fieldName} is required.");
        }

        if (userId.Trim().Length > MaxUserIdLength)
        {
            return ValidationResult.Fail($"{fieldName} cannot be longer than {MaxUserIdLength} characters.");
        }

        if (ContainsInvalidControlCharacter(userId))
        {
            return ValidationResult.Fail($"{fieldName} contains invalid characters.");
        }

        return ValidationResult.Success();
    }

    private static bool ContainsInvalidControlCharacter(string value)
    {
        return value.Any(character =>
            char.IsControl(character) &&
            character != '\t' &&
            character != '\n' &&
            character != '\r');
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
