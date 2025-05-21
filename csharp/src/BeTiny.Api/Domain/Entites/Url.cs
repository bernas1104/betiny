using BeTiny.Api.Domain.Common.Entities;
using BeTiny.Api.Domain.ValueObjects;

namespace BeTiny.Api.Domain.Entites
{
    public class Url : AggregateRoot<UrlId, string>
    {
        public string LongUrl { get; private set; }
        public Counter Accesses { get; private set; }

        public Url(UrlId id, string longUrl)
        {
            Id = id;
            LongUrl = longUrl;
            Accesses = new Counter();
        }
    }
}
