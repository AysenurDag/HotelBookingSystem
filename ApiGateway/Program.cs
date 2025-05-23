using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Yarp.ReverseProxy;
using System.Net.Http;

var builder = WebApplication.CreateBuilder(args);

// 🔐 JWT Authentication (Azure AD)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://login.microsoftonline.com/14ed001d-b0d4-4dcc-9446-0d74f496f1e4";
        options.Audience = "api://a891fc45-c1ce-4256-b116-7b83ab0c5204";
    });

// ✅ Authorization (isteğe bağlı)
builder.Services.AddAuthorization();

// 🔁 Rate Limiting (basit memory tabanlı)
builder.Services.AddRateLimiter(_ => _
    .AddFixedWindowLimiter("fixed", options =>
    {
        options.PermitLimit = 10; // dakikada 10 istek
        options.Window = TimeSpan.FromMinutes(1);
        options.QueueLimit = 0;
        options.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
    }));

// 📦 YARP config
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .ConfigureHttpClient((context, handler) =>
    {
        if (handler is SocketsHttpHandler socketsHandler)
        {
            socketsHandler.SslOptions.RemoteCertificateValidationCallback =
                (sender, cert, chain, sslPolicyErrors) => true;
        }
    });

var app = builder.Build();

// 🌐 Middleware pipeline
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapReverseProxy();

app.Run();
