namespace BeTiny.Api.Domain.Common.ValueObjects
{
    public abstract class AggregateRootId<TIdType> : ValueObject
    {
        public abstract TIdType Value { get; protected set; }
    }
}
