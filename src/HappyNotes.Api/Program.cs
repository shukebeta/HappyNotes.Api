using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Api.Framework;
using Api.Framework.Database;
using Api.Framework.Extensions;
using Api.Framework.Models;
using HappyNotes.Entities;
using HappyNotes.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NLog.Web;

var builder = WebApplication.CreateBuilder(args);
var logger = LoggerFactory.Create(config =>
{
    config.AddConsole();
    config.AddConfiguration(builder.Configuration.GetSection("Logging"));
}).CreateLogger("Program"); 

var envName = builder.Environment.EnvironmentName;
builder.Host.UseNLog();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    // Optionally configure other serialization options here
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Happy Notes API",
        Version = "v1"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description =
            "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwO\"",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});
builder.Services.AddSingleton(typeof(IRepositoryBase<>), typeof(RepositoryBase<>));
builder.Services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
builder.Services.AddSingleton<IAccountService, AccountService>();
builder.Services.AddSingleton<User>();
builder.Services.AddSqlSugarSetup(builder.Configuration.GetSection("DatabaseConnectionOptions")
    .Get<DatabaseConnectionOptions>()!, logger);
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

ConfigAuthentication(builder);
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
logger.LogInformation(envName);
app.Run();

void ConfigAuthentication(WebApplicationBuilder b)
{
    var services = b.Services;
    var configuration = b.Configuration;
    services.Configure<JwtConfig>(configuration.GetSection("Jwt"));
    var jwtConfig = configuration.GetSection("Jwt").Get<JwtConfig>();
    services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtConfig!.Issuer,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey =
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.SymmetricSecurityKey)),
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(5)
            };
            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = context =>
                {
                    var userContext = context.HttpContext.RequestServices.GetService<User>() ?? new User();
                    var claims = context.Principal?.Claims.ToArray();
                    if (claims != null)
                    {
                        userContext.Id = int.Parse(claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);
                        userContext.Username = claims.First(x => x.Type == ClaimTypes.Name).Value;
                        userContext.Email = claims.First(x => x.Type == ClaimTypes.Email).Value;
                    }

                    return Task.CompletedTask;
                }
            };
        });
    JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
}