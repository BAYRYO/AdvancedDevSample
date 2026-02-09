namespace AdvancedDevSample.Domain.Exceptions;

/// <summary>
/// Exception thrown when password does not meet security requirements.
/// </summary>
public class WeakPasswordException : DomainException
{
    public IReadOnlyList<string> ValidationErrors { get; }

    public WeakPasswordException(IEnumerable<string> errors)
        : base("Password does not meet security requirements: " + string.Join(" ", errors))
    {
        ValidationErrors = [.. errors];
    }
}
