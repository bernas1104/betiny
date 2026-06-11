using Microsoft.AspNetCore.HttpOverrides;
using BeTiny.IOC;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

var configuration = builder.Configuration;

builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.RegisterBindings(configuration);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | 
        ForwardedHeaders.XForwardedProto;
    
    // Only trust forwarded headers from known proxy/load balancer IPs
    // Example: options.KnownProxies.Add(IPAddress.Parse("proxy-ip-here"));
    // Or for Azure/AWS/GCP, configure KnownNetworks appropriately
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseForwardedHeaders();
app.UseHttpsRedirection();
app.MapControllers();

await app.RunAsync();

public partial class Program { }
