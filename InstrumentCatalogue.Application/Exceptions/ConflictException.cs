namespace InstrumentCatalogue.Application.Exceptions
{
    public class ConflictException : Exception
    {
        public string EntityName { get; init; }

        public string ConflictingField { get; init; }

        public string ClientMessage { get; init; }

        public ConflictException(string entity, string field, string clientMessage) : base($"{entity} encountered conflicts across the field/s {field} during the operation.")
        {
            EntityName = entity;
            ConflictingField = field;
            ClientMessage = clientMessage;
        }
    }
}
