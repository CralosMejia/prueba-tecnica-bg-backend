namespace ShoppingCart.Application.Common.Exceptions;

public class BusinessConflictException : Exception
{
    public BusinessConflictException(string message)
        : base(message)
    {
    }
}