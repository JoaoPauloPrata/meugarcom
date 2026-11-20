using Mediator;
using MeuGarcom.Api.Endpoints;
using MeuGarcom.Api.Http;
using MeuGarcom.Application;
using MeuGarcom.Application.Common.Behaviors;
using MeuGarcom.Application.Common.Interfaces;
using MeuGarcom.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;

DotEnvFile.Load(Directory.GetCurrentDirectory());

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediator((MediatorOptions options) =>
{
    options.ServiceLifetime = ServiceLifetime.Scoped;
    options.Assemblies = [typeof(MeuGarcom.Application.DependencyInjection)];
    options.PipelineBehaviors = [typeof(ValidationBehavior<,>)];
});
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ExceptionHandler>();

// Supabase Auth issues ES256 JWTs; keys are discovered from the issuer's OIDC metadata (JWKS).
var supabaseUrl = builder.Configuration["SUPABASE_URL"]
    ?? throw new InvalidOperationException("SUPABASE_URL is not configured.");
var issuer = $"{supabaseUrl.TrimEnd('/')}/auth/v1";
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = issuer;
        options.MapInboundClaims = false;
        options.TokenValidationParameters.ValidIssuer = issuer;
        options.TokenValidationParameters.ValidAudience = "authenticated";
    });
builder.Services.AddAuthorization();

var allowedOrigins = (builder.Configuration["CORS_ORIGINS"] ?? "http://localhost:5173")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy
    .WithOrigins(allowedOrigins)
    .AllowAnyHeader()
    .AllowAnyMethod()));

var app = builder.Build();

app.UseExceptionHandler();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => TypedResults.Ok(new { status = "ok" })).AllowAnonymous();
app.MapEndpoints();

app.Run();
