using Microsoft.EntityFrameworkCore;
using nigar_payment_service.Consumers;
using nigar_payment_service.DbContext;
using nigar_payment_service.Gateways;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// CORS
builder.Services.AddCors(opt =>
  opt.AddDefaultPolicy(policy =>
    policy.AllowAnyOrigin()
          .AllowAnyHeader()
          .AllowAnyMethod()));

// 1) EF Core
builder.Services.AddDbContext<PaymentDbContext>(options =>
  options.UseNpgsql(
    builder.Configuration.GetConnectionString("DefaultConnection")
  ));

// 2) RabbitMQ – config’den okumak için
var rabbit = builder.Configuration.GetSection("RabbitMQ");


builder.Services.AddSingleton<IConnectionFactory>(sp =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    return new ConnectionFactory
    {
        HostName = cfg.GetValue<string>("RabbitMQ:Host"),
        Port     = cfg.GetValue<int>("RabbitMQ:Port"),
        UserName = cfg.GetValue<string>("RabbitMQ:Username"),
        Password = cfg.GetValue<string>("RabbitMQ:Password"),
        DispatchConsumersAsync = true
    };
});


builder.Services.AddSingleton<IPaymentGateway, RuleBasedPaymentGateway>();

// BookingCreatedConsumer’ı Hosted Service olarak ekle
builder.Services.AddHostedService<BookingCreatedConsumer>();



// MVC + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 4) Startup’da otomatik migrate
using(var scope = app.Services.CreateScope())
{
  var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
  db.Database.Migrate();
}

app.UseCors();

if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.MapGet("/", () => "💳 Payment Service is running!");

app.Run();
