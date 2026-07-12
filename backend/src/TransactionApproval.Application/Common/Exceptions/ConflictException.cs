namespace TransactionApproval.Application.Common.Exceptions;

/// <summary>Thrown when a request conflicts with existing state (e.g. duplicate username).</summary>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }
}
