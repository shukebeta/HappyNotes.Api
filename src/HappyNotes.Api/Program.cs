using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;
using Api.Framework;
using Api.Framework.Database;
using Api.Framework.Extensions;
using Api.Framework.Models;
using HappyNotes.Api;
using HappyNotes.Dto;
using HappyNotes.Services;
using HappyNotes.Services.interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NLog.Web;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);
var logger = LoggerFactory.Create(config =>
{
    config.AddConsole();
    config.AddConfiguration(builder.Configuration.GetSection("Logging"));
}).CreateLogger("Program");

var envName = builder.Environment.EnvironmentName;
builder.Host.UseNLog();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    // Optionally configure other serialization options here
});
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(SetupSwaggerGen());
builder.Services.AddScoped(typeof(IRepositoryBase<>), typeof(RepositoryBase<>));
builder.Services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));

builder.Services.AddSqlSugarSetup(builder.Configuration.GetSection("DatabaseConnectionOptions")
    .Get<DatabaseConnectionOptions>()!, logger);

builder.Services.AddSingleton<HappyNotes.Services.ManticoreConnectionOptions>(builder.Configuration.GetSection("ManticoreConnectionOptions").Get<HappyNotes.Services.ManticoreConnectionOptions>()!);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddCors(SetupCors(builder));
ConfigAuthentication(builder);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.RegisterServices();

var app = builder.Build();
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Local"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowOrigins");
app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
logger.LogInformation(envName);
app.Run();
return;

///////////// main program ends here, and the following are local methods /////////////////////////////////////

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
        });

    JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
}

Action<SwaggerGenOptions> SetupSwaggerGen()
{
    return c =>
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
    };
}

Action<CorsOptions> SetupCors(WebApplicationBuilder webApplicationBuilder)
{
    return opts =>
    {
        string[] originList = webApplicationBuilder.Configuration.GetSection("AllowedCorsOrigins").Get<List<string>>()?.ToArray() ?? [];
        opts.AddPolicy("AllowOrigins", policy => policy.WithOrigins(originList)
            .AllowCredentials()
            .AllowAnyMethod()
            .AllowAnyHeader()
        );
    };
}
