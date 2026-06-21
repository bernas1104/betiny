using BeTiny.Application.Common.Models;
using Bogus;

namespace BeTiny.UnitTests.MotherObjects;

public static class IpApiResponseMotherObject
{
    public static IpApiResponse GetResponse(string? country = null) => new Faker<IpApiResponse>()
        .CustomInstantiator(f => new IpApiResponse
        (
            f.Internet.IpAddress()
                .MapToIPv4()
                .ToString(),
            f.Random.Bool(),
            country ?? f.Address.Country(),
            f.Address.Latitude(),
            f.Address.Longitude()
        ))
        .Generate();
}
