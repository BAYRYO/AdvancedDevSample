namespace AdvancedDevSample.Domain.Exceptions;

public class InvalidCredentialsException : DomainException
{
    public InvalidCredentialsException()
        : base("Email ou mot de passe incorrect.")
    {
    }
}
