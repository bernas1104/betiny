using BeTiny.Application.Common.Models;
using Bogus;

namespace BeTiny.UnitTests.MotherObjects;

public static class IpApiResponseMotherObject
{
    public static IpApiResponse GetResponse(string? country = null, bool? success = true) => 
        new Faker<IpApiResponse>()
            .CustomInstantiator(f => new IpApiResponse
            (
                f.Internet.IpAddress()
                    .MapToIPv4()
                    .ToString(),
                success ?? f.Random.Bool(),
                success ?? f.Random.Bool() ? f.Random.String2(10) : null,
                country ?? f.Address.Country(),
                f.Address.Latitude(),
                f.Address.Longitude()
            ))
            .Generate();
}
