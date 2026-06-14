using BeTiny.Infrastructure.Postgres.Context;
using Microsoft.EntityFrameworkCore;

namespace BeTiny.UnitTests.Fixtures;

public static class GenericRepositoryTestFixture
{
    public static BeTinyContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<BeTinyContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new BeTinyContext(options);
    }
}
