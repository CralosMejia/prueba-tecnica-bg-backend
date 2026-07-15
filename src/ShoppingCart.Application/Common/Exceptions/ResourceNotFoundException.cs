namespace ShoppingCart.Application.Common.Exceptions;

public sealed class ResourceNotFoundException : Exception
{
    public ResourceNotFoundException(
        string resourceName,
        object identifier)
        : base(
            $"{resourceName} with identifier '{identifier}' was not found."
        )
    {
    }
}