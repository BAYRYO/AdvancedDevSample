namespace AdvancedDevSample.Domain.Exceptions;

public class UserAlreadyExistsException : DomainException
{
    public string Email { get; }

    public UserAlreadyExistsException(string email)
        : base($"Un utilisateur avec l'adresse email '{email}' existe deja.")
    {
        Email = email;
    }
}
