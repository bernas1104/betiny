using BeTiny.Api.Domain.Common.ValueObjects;

namespace BeTiny.Api.Domain.ValueObjects
{
    public class Counter : ValueObject
    {
        public int Value { get; private set; }

        public Counter(int? value = null)
        {
            Value = value ?? 0;
        }

        public void Increment() => Value++;

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
