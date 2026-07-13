using System.Text.Json.Serialization;

namespace BeTiny.Application.Common.Models;

public record IpApiResponse(
    string Query,
    bool Success,
    string? Country,
    string? Message,
    [property: JsonPropertyName("lat")]
    double Latitude,
    [property: JsonPropertyName("lon")]
    double Longitude
);