namespace InstrumentCatalogue.Application.Exceptions
{
    public abstract class NotFoundException : Exception
    {
        public string EntityName { get; init; } = string.Empty;

        protected NotFoundException(string message) : base(message) { }



    }
    public class NotFoundException<TKey> : NotFoundException
    {
        

        public TKey Id {  get; init; }

        public NotFoundException(string entityName, TKey id) : base($"{entityName} with Id: {id} was not found")
        {
            EntityName = entityName;

            Id = id;

        }

    }
}
