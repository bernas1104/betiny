using BeTiny.Api.Filters;
using BeTiny.IOC;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

var configuration = builder.Configuration;

builder.Services.AddOpenApi();
builder.Services.AddControllers(opt => opt.Filters.Add<ResultsFilter>());
builder.Services.AddEndpointsApiExplorer();
builder.Services.RegisterBindings(configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();

await app.RunAsync();
