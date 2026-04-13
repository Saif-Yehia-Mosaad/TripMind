using System;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TripMind.API.Middleware;
using TripMind.Application.Interfaces;
using TripMind.Application.Services;
using TripMind.Infrastructure.Services;
using TripMind.Infrastructure.Email;
using TripMind.Infrastructure.Persistence;
using TripMind.Infrastructure.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TripMindDbContext>(opt =>
    opt.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.MigrationsAssembly("TripMind.Infrastructure")
          .CommandTimeout(30)
          .EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null)));

builder.Services.AddHttpClient<AiService>();
builder.Services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<TripMindDbContext>());
builder.Services.AddScoped<IJwtProvider,    JwtProvider>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
var emailProvider = builder.Configuration["Email:Provider"];
if (emailProvider == "Brevo")
{
    var apiKey = builder.Configuration["Email:ApiKey"]!;
    builder.Services.AddScoped<IEmailSender>(_ => new BrevoEmailSender(apiKey));
}
else
{
    builder.Services.AddScoped<IEmailSender, StubEmailSender>();
}

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<TripService>();
builder.Services.AddScoped<BudgetService>();
builder.Services.AddScoped<LocationService>();
builder.Services.AddScoped<ReviewService>();
builder.Services.AddScoped<ItineraryService>();
builder.Services.AddScoped<TourPackageService>();
builder.Services.AddScoped<IImageService, CloudinaryImageService>();

var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("Jwt:Secret is not configured.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt => opt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidateAudience         = true,
        ValidateLifetime         = true,
        ValidateIssuerSigningKey = true,
        ClockSkew                = TimeSpan.Zero,
        ValidIssuer              = builder.Configuration["Jwt:Issuer"],
        ValidAudience            = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
    });

builder.Services.AddAuthorization();

builder.Services.AddResponseCompression(opt =>
{
    opt.EnableForHttps = true;
    opt.Providers.Add<GzipCompressionProvider>();
    opt.Providers.Add<BrotliCompressionProvider>();
});

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(opt =>
    {
        opt.InvalidModelStateResponseFactory = ctx =>
        {
            var errors = ctx.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .ToDictionary(e => e.Key, e => e.Value!.Errors.Select(x => x.ErrorMessage).ToArray());
            return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(new
                { title = "Validation failed.", status = 400, errors });
        };
    });

builder.Services.AddCors(opt =>
    opt.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "TripMind API",
        Version     = "v1",
        Description = "AI-Driven Egyptian Domestic Tourism Planner"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization", Type = SecuritySchemeType.Http,
        Scheme = "bearer", BearerFormat = "JWT", In = ParameterLocation.Header
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {{
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
        },
        Array.Empty<string>()
    }});
});

builder.Services.AddHealthChecks().AddDbContextCheck<TripMindDbContext>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TripMindDbContext>();
    await db.Database.MigrateAsync();
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<RateLimitingMiddleware>();
app.UseResponseCompression();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "TripMind API v1");
    c.RoutePrefix = string.Empty;
});

app.Lifetime.ApplicationStarted.Register(() =>
{
    _ = Task.Run(async () =>
    {
        while (true)
        {
            await Task.Delay(TimeSpan.FromHours(1));
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TripMindDbContext>();
            var expired = await db.Users
                .Where(u => !u.IsEmailVerified && u.CreatedAt < DateTime.UtcNow.AddHours(-24))
                .ToListAsync();
            if (expired.Any())
            {
                db.Users.RemoveRange(expired);
                await db.SaveChangesAsync();
            }
        }
    });
});

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<AuditLogMiddleware>();

app.UseStaticFiles();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
