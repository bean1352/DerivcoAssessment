using DerivcoWebAPI.Database;
using DerivcoWebAPI.Services;
using SqliteDapper.Demo.Database;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddScoped<DatabaseConfig>();
builder.Services.AddScoped<IDatabaseBootstrap, DatabaseBootstrap>();
builder.Services.AddScoped<IRouletteService, RouletteService>();
//builder.Services.AddSwaggerGenNewtonsoftSupport();

builder.Services.AddControllers().AddJsonOptions(options =>
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Dean Braun Derivco Assessment",
        Description = "Roulette Web API",
        Contact = new OpenApiContact
        {
            Name = "Dean Braun GitHub",
            Email = "braundean11@gmail.com",
            Url = new Uri("https://github.com/bean1352")
        },
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseMiddleware<ErrorHandlerMiddleware>();

app.Run();
