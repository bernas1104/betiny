using BeTiny.Api.Domain.Common.Entities;
using BeTiny.Api.Domain.ValueObjects;

namespace BeTiny.Api.Domain.Entites
{
    public class Url : AggregateRoot<UrlId, string>
    {
        public string LongUrl { get; private set; }
        public Counter Accesses { get; private set; }

        #pragma warning disable CS8618
        /// <summary>
        /// Private empty constructor needed by EF Core
        /// </summary>
        private Url()
        {
        }
        #pragma warning restore

        public Url(UrlId id, string longUrl)
        {
            Id = id;
            LongUrl = longUrl;
            Accesses = new Counter();
            CreatedAt = DateTime.UtcNow;
        }
    }
}
