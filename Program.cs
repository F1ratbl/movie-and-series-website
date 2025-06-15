using FilmDiziSitesi.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 🚀 MVC yapısı için Controller + View desteği ekleniyor
builder.Services.AddControllersWithViews();

// 🗃️ Entity Framework - SQLite veritabanı bağlantısı ayarlanıyor
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// 🔐 Authentication servisi ekleniyor (Cookie tabanlı kimlik doğrulama)
builder.Services.AddAuthentication("MyCookieAuth")
    .AddCookie("MyCookieAuth", options =>
    {
        options.LoginPath = "/Account/Login";         // Giriş yapılmamışsa yönlendirilecek sayfa
        options.AccessDeniedPath = "/Account/AccessDenied";  // Yetkisiz erişim sayfası
    });

// 🔐 Authorization servisi ekleniyor (roller ve politikalar için)
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));       // Sadece "Admin" rolü olan kullanıcılar erişebilir
});

// 🛠️ Uygulama oluşturuluyor
var app = builder.Build();

// 🌐 Geliştirme ortamında değilsek hata sayfası ve güvenlik ayarları etkinleştiriliyor
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error"); // Hatalar için özel sayfa
    app.UseHsts();                          // Tarayıcıya HTTPS kullanmasını önerir
}

// 🔐 HTTPS yönlendirmesi zorunlu kılınıyor
app.UseHttpsRedirection();

// 🖼️ wwwroot klasöründen statik dosyalar (css/js/img vs.) servis edilir
app.UseStaticFiles();

// 🧭 Rotalama (hangi URL hangi controller'a gider?)
app.UseRouting();

// 🔐 Authentication middleware mutlaka Authorization'dan önce gelmeli
app.UseAuthentication();

// 🔐 Yetkilendirme middleware ([Authorize] attribute için)
app.UseAuthorization();

// 🧭 Varsayılan rota: /Home/Index
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ▶️ Uygulama başlatılıyor
app.Run();