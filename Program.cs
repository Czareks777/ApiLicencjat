using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using LicencjatUG.Server.Data;
using LicencjatUG.Server.Data.Seeders;
using LicencjatUG.Server.Hubs;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });
builder.WebHost.UseKestrel();

// Konfiguracja bazy danych
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Konfiguracja JWT
var keyString = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(keyString) || keyString.Length < 32)
{
    throw new Exception("BŁĄD: Klucz JWT jest za krótki! Upewnij się, że ma co najmniej 32 znaki.");
}
var key = Encoding.UTF8.GetBytes(keyString);


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true
        };
    });

builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Licencjat", policy =>
    {
        policy.SetIsOriginAllowed(origin => origin == "http://localhost:4200" || origin == "null")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Konfiguracja Swaggera z obsługą JWT
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Licencjat API",
        Version = "v1"
    });

    // Dodajemy definicję bezpieczeństwa (JWT Bearer)
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\nPodaj token w formacie: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // Dodajemy wymaganie bezpieczeństwa dla wszystkich endpointów
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

var app = builder.Build();

// **Uruchamiamy migracje i seedowanie danych**
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<DataContext>();

    try
    {
        Console.WriteLine("🔄 Migracja bazy danych...");
        context.Database.Migrate();

        Console.WriteLine("🔄 Seedowanie zespołów...");
        await TeamSeeder.SeedTeamsAsync(context); // najpierw seedujemy zespół

        Console.WriteLine("🔄 Seedowanie użytkowników...");
        await UserSeeder.SeedUsersAsync(context); // później użytkowników

        Console.WriteLine("🔄 Seedowanie zadań...");
        await TaskSeeder.SeedTasksAsync(context);

        Console.WriteLine("✅ Seedowanie zakończone!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Błąd podczas migracji lub seedowania: {ex.Message}");
    }
}

app.UseRouting();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Licencjat API V1");
    c.RoutePrefix = "swagger";
});

app.UseDefaultFiles();
app.MapStaticAssets();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    // W developmentie nie przekierowujemy do HTTPS
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors("Licencjat");
app.UseAuthentication();
app.UseAuthorization();
app.MapHub<ChatHub>("/chathub");
app.MapControllers();
app.MapFallbackToFile("/index.html");

app.Run();
