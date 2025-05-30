using FilmDiziSitesi.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 🔧 Servisler ekleniyor
builder.Services.AddControllersWithViews();

// 🔗 DbContext bağlantısı (SQLite)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// 🔍 Swagger (opsiyonel ama önerilir - API testleri için)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "veritabani.db");
Console.WriteLine($"Kullanılan veritabanı: {dbPath}");

var app = builder.Build();

// 🌐 Orta katmanlar (Middleware)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts(); // Güvenlik için önerilir
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();     // HTTP -> HTTPS yönlendirmesi
app.UseStaticFiles();          // wwwroot içeriği için

app.UseRouting();              // Rotalama sistemi
app.UseAuthorization();        // Yetkilendirme (ileride lazım olabilir)

// 🌐 MVC rotası
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
