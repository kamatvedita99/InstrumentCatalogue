namespace InstrumentCatalogue.Core.Exceptions;

public class InvalidCursorException : Exception
{
    public InvalidCursorException(Exception innerException)
        : base("Cursor decoding failed.", innerException) { }

    public InvalidCursorException()
        : base("Cursor decoding failed.") { }
}
