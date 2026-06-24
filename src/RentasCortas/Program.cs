using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using RentasCortas.Common.Middleware;
using RentasCortas.Common.Security;
using RentasCortas.Data;
using RentasCortas.Common.Storage;
using RentasCortas.Features.Auth;
using RentasCortas.Features.Inmuebles;
using RentasCortas.Features.Reservas;
using RentasCortas.Features.Favoritos;
using RentasCortas.Features.KYC;
using RentasCortas.Features.Notificaciones;
using RentasCortas.Common.Notifications;
using RentasCortas.Common.Background;
using RentasCortas.Features.Dashboard;
using RentasCortas.Features.Reportes;
using RentasCortas.Common.Responses;
using Microsoft.AspNetCore.Mvc;


var builder = WebApplication.CreateBuilder(args);

// --- Base de datos ---
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- Autenticación JWT ---
var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// --- Servicios comunes ---
builder.Services.AddSingleton<JwtHelper>();

// --- Features ---
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IInmueblesService, InmueblesService>();
builder.Services.AddSingleton<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<IReservasService, ReservasService>();
builder.Services.AddScoped<IFavoritosService, FavoritosService>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<EmailNotificationService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IKYCService, KYCService>();
builder.Services.AddScoped<INotificacionesService, NotificacionesService>();
builder.Services.AddHostedService<CheckoutReminderService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IReportesService, ReportesService>();

// --- Controllers ---
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .SelectMany(e => e.Value!.Errors.Select(err => err.ErrorMessage))
                .ToList();

            var message = errors.Count > 0
                ? string.Join(" | ", errors)
                : "Datos de entrada inválidos";

            return new BadRequestObjectResult(new ApiResponse(message, 400));
        };
    });
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Components ??= new();
        document.Components.SecuritySchemes["Bearer"] = new()
        {
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Ingresa tu token JWT"
        };
        document.SecurityRequirements.Add(new()
        {
            [new()
            {
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                Reference = new()
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            }] = Array.Empty<string>()
        });
        return Task.CompletedTask;
    });
});

var app = builder.Build();

// --- Middleware ---
app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Authentication = new ScalarAuthenticationOptions
        {
            PreferredSecuritySchemes = ["Bearer"]
        };
    });
}

app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();