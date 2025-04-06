using auth_user_service.Data;
using auth_user_service.Messaging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// SQL Server için EF Core ve veritabanı bağlantısı
builder.Services.AddDbContext<AuthUserDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Swagger/OpenAPI servisi ekleniyor
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Mesajlaşma altyapısı: Publisher'ı singleton, Consumer'ı background service olarak ekliyoruz
builder.Services.AddSingleton<MessagePublisher>();
builder.Services.AddHostedService<MessageConsumerService>();

// Controller tabanlı API kullanıyorsanız, controller'ları ekleyin
builder.Services.AddControllers();

var app = builder.Build();

// Geliştirme ortamında Swagger arayüzünü etkinleştiriyoruz
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Controller endpoint'lerini haritalıyoruz
app.MapControllers();

app.Run();