using System.Text.Json.Serialization;
using ArtemisBanking.Core.Application;
using ArtemisBanking.Infrastructure.Identity;
using ArtemisBanking.Infrastructure.Persistence;
using ArtemisBanking.Infrastructure.Shared;
using ArtemisBankingApi.Extensions;
using Hangfire;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(opt =>
{
    opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});;
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

// Registrar capas de la aplicación
builder.Services.AddApplicationLayerIoc(builder.Configuration);
builder.Services.AddPersistenceLayerIoc(builder.Configuration);
builder.Services.AddIdentityLayerIocForWebApi(builder.Configuration);
builder.Services.AddSharedLayerIoc(builder.Configuration);
builder.Services.AddHangfireConfiguration(builder.Configuration);

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer(); // Ayuda configurar metada para documentacion (para swagger en realidad)
builder.Services.AddHealthChecks(); // Diagnostica el estado de salud
// esto construye swagger
builder.Services.AddAppiVersioningExtension(); // Metodo de extension
builder.Services.AddSwaggerExtension(); // metodo de extension

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

// CORS configuration (si es necesario para desarrollo)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();
app.UseHangFireJobs();
app.UseHangfireDashboard("/hangfire");
await app.Services.RunIdentitySeedAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerExtension(app);
    app.MapOpenApi();
    app.UseCors("AllowAll");
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseHealthChecks("/health"); // Una URL ara consultar la salud de la API

app.MapControllers();

app.Run();