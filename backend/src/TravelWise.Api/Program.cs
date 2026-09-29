using Microsoft.AspNetCore.Diagnostics;
using Serilog;
using TravelWise.Infrastructure;
using TravelWise.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TravelWise.Application.Common.Options;
using FluentValidation;
using FluentValidation.AspNetCore;
using TravelWise.Api.Auth;

var builder = WebApplication.CreateBuilder(args);

// ---------- Logging: Serilog ----------
builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console();
});

// ---------- Core services ----------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "TravelWise API",
        Version = "v1",
        Description = "TravelWise — Intelligent Travel Resilience Platform"
    });

    // JWT bearer security definition
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter your JWT access token (without the 'Bearer' prefix)."
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ---------- FluentValidation ----------
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<TravelWise.Application.Features.Auth.Validators.RegisterRequestValidator>();

// ---------- JWT + Service Token Authentication ----------
var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
var jwtOptions = jwtSection.Get<JwtOptions>()
    ?? throw new InvalidOperationException("Jwt configuration section is missing.");

var serviceToken = builder.Configuration["AiService:Token"]
    ?? throw new InvalidOperationException("AiService:Token configuration is missing.");

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    })
    .AddScheme<ServiceTokenAuthOptions, ServiceTokenAuthHandler>(
        ServiceTokenAuthHandler.SchemeName,
        opts => opts.ExpectedToken = serviceToken);

builder.Services.AddAuthorization();

// ---------- Infrastructure (EF Core + PostgreSQL) ----------
builder.Services.AddInfrastructure(builder.Configuration);

// ---------- CORS for future React + Flutter dev ----------
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",
                "http://localhost:3000",
                "http://localhost:8080")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// ---------- Health checks ----------
builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<TravelWiseDbContext>("database");

var app = builder.Build();

// ---------- Global exception handler ----------
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var feature = context.Features.Get<IExceptionHandlerFeature>();
        var error = feature?.Error;

        // Map AppException to its status code
        if (error is TravelWise.Application.Common.Exceptions.AppException appEx)
        {
            context.Response.StatusCode = appEx.StatusCode;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(new
            {
                type = $"https://httpstatuses.com/{appEx.StatusCode}",
                title = appEx.Message,
                status = appEx.StatusCode
            });
            return;
        }

        // Everything else -> 500
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(new
        {
            type = "https://httpstatuses.com/500",
            title = "An unexpected error occurred.",
            status = 500,
            detail = app.Environment.IsDevelopment() ? error?.Message : null
        });
    });
});

// ---------- Pipeline ----------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseCors("DevCors");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// Seed data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TravelWiseDbContext>();
    await db.Database.MigrateAsync();
    await TravelWise.Infrastructure.Persistence.Seed.SeedData.ApplyAsync(db);
}

app.Run();