namespace TransactionApproval.Application.Common.Exceptions;

/// <summary>Thrown when credentials are invalid during login.</summary>
public class AuthenticationException : Exception
{
    public AuthenticationException(string message) : base(message)
    {
    }
}
