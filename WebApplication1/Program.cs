using Microsoft.OpenApi;
using WebApplication1.Process;
using WebApplication1.Process.Contract;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Add Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddControllers();
// Register process layer services by contract
builder.Services.AddSingleton<IRpmTracker, RpmTracker>();
builder.Services.AddScoped<IBatchProcessor, BatchProcessor>();
builder.Services.AddScoped<IFileWriterService, FileWriterService>();

// Add Swagger/OpenAPI (Swashbuckle)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Rate Limiter API", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Rate Limiter API v1");
        c.RoutePrefix = "swagger"; // serve at /swagger
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
