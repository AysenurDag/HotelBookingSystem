using System.Reflection;
using auth_user_service.Configuration;
using auth_user_service.Data;
using auth_user_service.Messaging;
using auth_user_service.Repositories;
using auth_user_service.Sagas;
using Microsoft.EntityFrameworkCore;



var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<UserRegistrationSaga>();

// RabbitMQ ayarları
builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection("RabbitMq"));

// Ortama göre seçim
if (builder.Environment.IsEnvironment("Test"))
{
    builder.Services.AddSingleton<InMemoryQueuePublisher>();
    builder.Services.AddSingleton<IMessagePublisher>(sp =>
        sp.GetRequiredService<InMemoryQueuePublisher>());
}
else
{
    builder.Services.AddSingleton<RabbitMqPublisher>();
    builder.Services.AddSingleton<IMessagePublisher>(sp =>
        sp.GetRequiredService<RabbitMqPublisher>());

    builder.Services.AddHostedService<MessageConsumerService>();
}

// using auth_user_service.Repositories;
builder.Services.AddScoped<IUserRepository, EfUserRepository>();
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