using BeTiny.Application.Common.Models;
using Bogus;

namespace BeTiny.UnitTests.MotherObjects;

public static class IpApiResponseMotherObject
{
    public static IpApiResponse GetResponse(string? country = null, bool? success = true) => 
        new Faker<IpApiResponse>()
            .CustomInstantiator(f => {
                var isSuccess = success ?? f.Random.Bool();

                return new IpApiResponse
                (
                    f.Internet.IpAddress()
                        .MapToIPv4()
                        .ToString(),
                    isSuccess,
                    country ?? f.Address.Country(),
                    !isSuccess ? f.Random.String2(10) : null,
                    f.Address.Latitude(),
                    f.Address.Longitude()
                );
            })
            .Generate();
}
