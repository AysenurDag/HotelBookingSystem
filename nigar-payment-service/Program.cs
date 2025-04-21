using Microsoft.EntityFrameworkCore;
using nigar_payment_service.Consumers;
using nigar_payment_service.DbContext;
using nigar_payment_service.Gateways;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(opt =>
    opt.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod()));

//  EF Core
builder.Services.AddDbContext<PaymentDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

//  RabbitMQ ConnectionFactory 
builder.Services.AddSingleton<IConnectionFactory>(_ =>
    new ConnectionFactory
    {
        //HostName            = "10.47.7.151",
        HostName            = "localhost",
        Port                = 5672,
        UserName            = "guest",
        Password            = "guest",
        DispatchConsumersAsync = true
    });

// Hosted Service (consumer)
//builder.Services.AddHostedService<ReservationCreatedConsumer>();


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IPaymentGateway, RuleBasedPaymentGateway>();


var app = builder.Build();

app.UseCors();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.MapControllers();
app.MapGet("/", () => "💳 Payment Service is running!");

app.Run();
