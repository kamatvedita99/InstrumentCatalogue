namespace InstrumentCatalogue.Core.Exceptions;

public class ConflictException : Exception
{
    public string ClientMessage { get; init; }

    public ConflictException(string clientMessage, Exception? innerException) : base("A conflict occurred while saving data. It may already exist or violate a business constraint.", innerException)
    {
        ClientMessage = clientMessage;
    }
}
