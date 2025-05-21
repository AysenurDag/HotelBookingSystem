using System.Security.Claims;
using auth_user_service.Data;
using auth_user_service.Middlewares;
using auth_user_service.Models;
using auth_user_service.Repositories;
using auth_user_service.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 1) DbContext
builder.Services.AddDbContext<AuthUserDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()
    )
);

// 2) Redis
builder.Services.AddSingleton<RedisService>();

// 3) IdentityCore
builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 6;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AuthUserDbContext>()
    .AddDefaultTokenProviders();

// 4) CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", policy =>
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
    );
});

// 5) Authentication & JWT Bearer
var azureAd = builder.Configuration.GetSection("AzureAd");
string tenantId = azureAd["TenantId"];
string ciamAuthority = $"https://hotelbookingext.ciamlogin.com/{tenantId}/v2.0";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = ciamAuthority;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuers = new[]
            {
                $"https://login.microsoftonline.com/{tenantId}/v2.0",
                ciamAuthority
            },
            ValidAudiences = new[]
            {
                azureAd["Audience"],
                azureAd["ClientId"]
            }
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var userManager = context.HttpContext.RequestServices
                    .GetRequiredService<UserManager<ApplicationUser>>();
                var claimsIdentity = context.Principal.Identity as ClaimsIdentity;

                var email = claimsIdentity?.FindFirst("preferred_username")?.Value;
                if (email == null) return;

                var user = await userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        Email = email,
                        UserName = email,
                        Name = claimsIdentity.FindFirst("name")?.Value ?? ""
                    };
                    await userManager.CreateAsync(user);
                    await userManager.AddToRoleAsync(user, "User");
                }
            }
        };
    });

// 6) Authorization
builder.Services.AddAuthorization(options =>
    options.AddPolicy("RequireAdmin", policy =>
        policy.RequireRole("Admin"))
);

// 7) Diğer servisler
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<IApplicationUserRepository, ApplicationUserRepository>();
builder.Services.AddMemoryCache();

// 8) Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var scopeUri = azureAd["Scopes"];
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "AuthUserService API", Version = "v1" });

    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri($"{azureAd["Instance"]}{tenantId}/oauth2/v2.0/authorize"),
                TokenUrl = new Uri($"{azureAd["Instance"]}{tenantId}/oauth2/v2.0/token"),
                Scopes = new Dictionary<string, string>
                {
                    { scopeUri, "Access Auth API" }
                }
            }
        }
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "oauth2"
                }
            },
            new[] { scopeUri }
        }
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AuthUserDbContext>();
    db.Database.Migrate();
}

// 9) Middleware sıralaması
app.UseRouting();
app.UseCors("AllowReact");
app.UseMiddleware<TokenBlacklistMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

// 10) Swagger UI
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "AuthUserService v1");
    c.OAuthClientId(azureAd["ClientId"]);
    c.OAuthUsePkce();
    c.OAuthScopeSeparator(" ");
    c.OAuthScopes(azureAd["Scopes"]);
    c.OAuthAdditionalQueryStringParams(new Dictionary<string, string>
    {
        { "p", "HotelBookingSignUp" }
    });
});

// 11) Rolleri otomatik oluştur
using (var scope = app.Services.CreateScope())
{
    var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    foreach (var role in new[] { "Admin", "User", "HotelOwner" })
    {
        if (!await roleMgr.RoleExistsAsync(role))
            await roleMgr.CreateAsync(new IdentityRole(role));
    }
}

app.MapControllers();
app.Run();
