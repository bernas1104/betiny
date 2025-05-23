using System.Text.Json.Serialization;

using BeTiny.Api.Domain.Common.ValueObjects;

namespace BeTiny.Api.Domain.ValueObjects
{
    public class Counter : ValueObject
    {
        public long Value { get; private set; }

        public Counter()
        {
            Value = default;
        }

        [JsonConstructor]
        public Counter(long value)
        {
            Value = value;
        }

        public void Increment() => Value++;

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
