using BeTiny.Api.Domain.Common.Utils;
using BeTiny.Api.Domain.Common.ValueObjects;

namespace BeTiny.Api.Domain.ValueObjects
{
    public class UrlId : AggregateRootId<string>
    {
        public override string Value { get; protected set; }

        protected UrlId(string value)
        {
            Value = value;
        }

        public static UrlId Create(string value) => new UrlId(value);

        public static UrlId CreateUnique(long value) =>
            new UrlId(Base62Encoder.Encode(value));

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString()
        {
            return Value;
        }
    }
}
