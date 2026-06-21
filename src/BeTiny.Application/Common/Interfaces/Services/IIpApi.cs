using BeTiny.Application.Common.Models;
using Refit;

namespace BeTiny.Application.Common.Interfaces.Services;

public interface IIpApi
{
    [Get("/json/{query}")]
    Task<IpApiResponse> GetIpInfoAsync(string query);
}
