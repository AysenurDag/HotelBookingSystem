using Microsoft.EntityFrameworkCore;
using nigar_payment_service.Consumers;
using nigar_payment_service.DbContext;
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
        HostName            = "10.47.7.151",
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

var app = builder.Build();

app.UseCors(); 

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapControllers();

// Optional health‐check or root
app.MapGet("/", () => "💳 Payment Service is running!");

app.Run();