using GardensBack.Application;
using GardensBack.Application.DeviceProvisioning.Configuration;
using GardensBack.Application.FirmwareReleases.Configuration;
using GardensBack.Application.FirmwareUpdates.Configuration;
using GardensBack.Authentication;
using GardensBack.Domain;
using GardensBack.Health;
using GardensBack.Infrastructure.Persistance;
using GardensBack.Middleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using System.Reflection;
using System.Text.Json.Serialization;

if (args.Contains("--healthcheck"))
{
    using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(4) };
    try
    {
        var response = await client.GetAsync("http://localhost:8080/health");
        Environment.ExitCode = response.IsSuccessStatusCode ? 0 : 1;
    }
    catch { Environment.ExitCode = 1; }
    return;
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Registro de estudiantes API",
        Version = "v1",
        Description = "API para el registro de estudiantes. Fechas en UTC."
    });
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Ingrese el accessToken obtenido en POST /api/auth/login."
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

builder.Services.AddInfrastructure(builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Configure ConnectionStrings:DefaultConnection."));
builder.Services.AddApplication();
builder.Services.AddHealthChecks().AddCheck<DatabaseHealthCheck>("database");
builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddOptions<DeviceProvisioningOptions>()
    .Bind(builder.Configuration.GetSection(DeviceProvisioningOptions.SectionName))
    .Validate(options => options.TokenExpirationMinutes is >= 1 and <= 60,
        "DeviceProvisioning:TokenExpirationMinutes debe estar entre 1 y 60.")
    .ValidateOnStart();

builder.Services.AddOptions<RabbitMqOptions>()
    .Bind(builder.Configuration.GetSection(RabbitMqOptions.SectionName))
    .Validate(options => !string.IsNullOrWhiteSpace(options.Host),
        "RabbitMQ:Host es obligatorio.")
    .Validate(options => options.MqttPort is >= 1 and <= 65535,
        "RabbitMQ:MqttPort debe ser un puerto válido.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.Username),
        "RabbitMQ:Username es obligatorio.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.Password),
        "RabbitMQ:Password es obligatorio.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.InventoryConsumerClientId),
        "RabbitMQ:InventoryConsumerClientId es obligatorio.")
    .ValidateOnStart();

builder.Services.AddOptions<FirmwareReleaseValidationOptions>()
    .Bind(builder.Configuration.GetSection(FirmwareReleaseValidationOptions.SectionName))
    .Validate(options => !string.IsNullOrWhiteSpace(options.PublicKeyPem),
        "FirmwareReleaseValidation:PublicKeyPem es obligatorio.")
    .Validate(options => options.DownloadTimeoutSeconds is >= 1 and <= 300,
        "FirmwareReleaseValidation:DownloadTimeoutSeconds debe estar entre 1 y 300.")
    .Validate(options => options.MaximumFirmwareSizeBytes is >= 1 and <= uint.MaxValue,
        "FirmwareReleaseValidation:MaximumFirmwareSizeBytes debe estar entre 1 y uint.MaxValue.")
    .ValidateOnStart();

builder.Services.AddOptions<FirmwareUpdateOptions>()
    .Bind(builder.Configuration.GetSection(FirmwareUpdateOptions.SectionName))
    .Validate(options => options.CommandExpirationMinutes is >= 1 and <= 60,
        "FirmwareUpdate:CommandExpirationMinutes debe estar entre 1 y 60.")
    .ValidateOnStart();

builder.Services.AddOptions<DeviceCommandSecurityOptions>()
    .Bind(builder.Configuration.GetSection(DeviceCommandSecurityOptions.SectionName))
    .Validate(options =>
    {
        if (string.IsNullOrWhiteSpace(options.EncryptionKey))
            return false;

        try
        {
            return Convert.FromBase64String(options.EncryptionKey).Length == 32;
        }
        catch (FormatException)
        {
            return false;
        }
    }, "DeviceCommandSecurity:EncryptionKey debe contener 32 bytes codificados en Base64.")
    .ValidateOnStart();

var app = builder.Build();

if (app.Configuration.GetValue<bool>("Database:InitializeOnStartup"))
{
    app.Logger.LogInformation("Aplicando migraciones de la base de datos.");
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (!db.Database.GetMigrations().Any())
        throw new InvalidOperationException("No hay migraciones. Cree la migración inicial antes de habilitar Database:InitializeOnStartup.");
    await db.Database.MigrateAsync();
    app.Logger.LogInformation("Base de datos lista. Iniciando la API.");
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseStatusCodePages();
app.UseSwagger();
app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "Gardens API v1"));

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health").AllowAnonymous();
app.MapGet("/", () => Results.Redirect("/swagger")).AllowAnonymous().ExcludeFromDescription();

app.Run();
