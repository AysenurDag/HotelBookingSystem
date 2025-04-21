using System.Reflection;
using auth_user_service.Data;
using auth_user_service.Messaging;
using auth_user_service.Repositories;
using Microsoft.EntityFrameworkCore;



var builder = WebApplication.CreateBuilder(args);

// using auth_user_service.Repositories;
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();


// SQL Server için EF Core ve veritabanı bağlantısı
builder.Services.AddDbContext<AuthUserDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null
            );
        })
);

// -- Mock repo
//builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();

// -- RabbitMQ publisher & consumer
builder.Services.AddSingleton<MessagePublisher>();
builder.Services.AddHostedService<MessageConsumerService>();

// -- Controller’lar
builder.Services.AddControllers();

// Swagger/OpenAPI servisi ekleniyor
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Proje derlendiğinde oluşan XML belgesinin adını ve yolunu alıyoruz
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    // Swagger’a XML yorumları dahil et
    c.IncludeXmlComments(xmlPath);
});

builder.Services.AddSingleton<MessagePublisher>();
builder.Services.AddHostedService<MessageConsumerService>();
try
{
    var producer = new MessageProducerService();
    producer.SendMessage("Merhaba, bu bir test mesajıdır!");
    producer.Close();
}
catch (Exception ex)
{
    Console.WriteLine($"RabbitMQ bağlantı hatası (yoksayar): {ex.Message}");
}




var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Auth API V1");
});
app.UseHttpsRedirection();

// Controller endpoint'lerini haritalıyoruz
app.MapControllers();

app.Run();