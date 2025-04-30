using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace authuserservice.Tests
{
    public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Mevcut EF Core kayıtlarını bulup kaldırıyoruz
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AuthUserDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Test ortamı için in-memory veritabanı ekliyoruz
                services.AddDbContext<AuthUserDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDB");
                });

                // Gerekirse, servis sağlayıcıyı yeniden inşa edebilirsiniz
                var sp = services.BuildServiceProvider();

                // İsteğe bağlı: veritabanını oluşturup, test verisi ekleyebilirsiniz.
                using (var scope = sp.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<AuthUserDbContext>();
                    db.Database.EnsureCreated();
                    // Test verilerini burada ekleyebilirsiniz.
                }
            });
        }
    }
}
