using auth_user_service.Data;
using auth_user_service.Middlewares;
using auth_user_service.Models;
using auth_user_service.Repositories;
using auth_user_service.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 1) DbContext
builder.Services.AddDbContext<AuthUserDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2) Redis
builder.Services.AddSingleton<RedisService>();

// 3) IdentityCore: sadece EF tabanlı User/Role, cookie middleware yüklemez
builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 6;
        // … diğer parola kuralları
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AuthUserDbContext>()
    .AddDefaultTokenProviders();

// 4) Authentication: Default scheme olarak JWT Bearer (hem Authenticate hem Challenge)
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

// 5) Authorization (rol tabanlı politika)
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy =>
        policy.RequireRole("Admin"));
});

// 6) Diğer servisler
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<IApplicationUserRepository, ApplicationUserRepository>();
builder.Services.AddMemoryCache();

// 7) Controllers + Swagger
var azureAd = builder.Configuration.GetSection("AzureAd");
var scopeUri = azureAd["Scopes"];  // örn. "api://<ClientId>/access_as_user"

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "AuthUserService API", Version = "v1" });

    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri($"{azureAd["Instance"]}{azureAd["TenantId"]}/oauth2/v2.0/authorize"),
                TokenUrl = new Uri($"{azureAd["Instance"]}{azureAd["TenantId"]}/oauth2/v2.0/token"),
                Scopes = new Dictionary<string, string> { { scopeUri, "Access Auth API" } }
            }
        }
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" } },
            new[] { scopeUri }
        }
    });
});

var app = builder.Build();

// 8) Middleware sıralaması
app.UseRouting();
app.UseMiddleware<TokenBlacklistMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

// 9) Swagger + PKCE destekli authorize butonu
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "AuthUserService v1");
    c.OAuthClientId(azureAd["ClientId"]);
    c.OAuthUsePkce();
    c.OAuthScopeSeparator(" ");
});

// 10) Rolleri otomatik oluştur
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    string[] roles = { "Admin", "User", "HotelOwner" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }
}

// 11) Controller’ları eşle ve çalıştır
app.MapControllers();
app.Run();
