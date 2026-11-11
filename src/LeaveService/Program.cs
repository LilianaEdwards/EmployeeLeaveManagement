
using System.Security.Claims;
using System.Text;
using LeaveService.Data;
using LeaveService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException(
        "JWT signing key is not configured.");

var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException(
        "JWT issuer is not configured.");

var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException(
        "JWT audience is not configured.");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        JwtBearerDefaults.AuthenticationScheme,
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter your JWT access token."
        });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecuritySchemeReference(
                    JwtBearerDefaults.AuthenticationScheme,
                    document),
                new List<string>()
            }
        });
});

var databaseProvider =
    builder.Configuration["DatabaseProvider"] ?? "SqlServer";

var leaveConnectionString =
    builder.Configuration.GetConnectionString("LeaveDatabase")
    ?? throw new InvalidOperationException(
        "LeaveDatabase connection string is not configured.");

builder.Services.AddDbContext<LeaveDbContext>(options =>
{
    if (databaseProvider.Equals(
        "PostgreSQL",
        StringComparison.OrdinalIgnoreCase))
    {
        options.UseNpgsql(leaveConnectionString);
    }
    else if (databaseProvider.Equals(
        "SqlServer",
        StringComparison.OrdinalIgnoreCase))
    {
        options.UseSqlServer(leaveConnectionString);
    }
    else
    {
        throw new InvalidOperationException(
            $"Unsupported database provider: {databaseProvider}");
    }
});

builder.Services.AddScoped<LeaveManager>();

builder.Services.AddHttpClient<EmployeeApiClient>(client =>
{
    var baseUrl = builder.Configuration[
        "Services:EmployeeService:BaseUrl"];

    if (string.IsNullOrWhiteSpace(baseUrl))
    {
        throw new InvalidOperationException(
            "EmployeeService BaseUrl is not configured.");
    }

    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(5);
});

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,

                ValidateAudience = true,
                ValidAudience = jwtAudience,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtKey)),

                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,

                NameClaimType = ClaimTypes.Name,
                RoleClaimType = ClaimTypes.Role
            };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

await ApplyDatabaseMigrationsAsync(app);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (builder.Configuration.GetValue<bool>(
    "HttpsRedirection:Enabled", true))
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

static async Task ApplyDatabaseMigrationsAsync(
    WebApplication app)
{
    const int maxAttempts = 15;

    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            using var scope = app.Services.CreateScope();

            var dbContext = scope.ServiceProvider
                .GetRequiredService<LeaveDbContext>();

            await dbContext.Database.MigrateAsync();

            app.Logger.LogInformation(
                "Leave database migrations applied successfully.");

            return;
        }
        catch (Exception exception) when (attempt < maxAttempts)
        {
            app.Logger.LogWarning(
                exception,
                "Leave database migration attempt {Attempt}/{MaxAttempts} failed. Retrying in 5 seconds.",
                attempt,
                maxAttempts);

            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }

    throw new InvalidOperationException(
        "Could not apply Leave database migrations.");
}
