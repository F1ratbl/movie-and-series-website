using Microsoft.AspNetCore.Mvc;
using FilmDiziSitesi.Models;
using FilmDiziSitesi.Data;
using FilmDiziSitesi.Services;

namespace FilmDiziSitesi.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMovieApiService _movieApiService;

        public AdminController(ApplicationDbContext context, IMovieApiService movieApiService)
        {
            _context = context;
            _movieApiService = movieApiService;
        }

        // Admin giriş kontrolü
        private bool IsAdmin()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null) return false;

            var user = _context.Users.FirstOrDefault(u => u.Id == int.Parse(userId));
            return user?.Role == "Admin";
        }

        // GET: Admin/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: Admin/Login
        [HttpPost]
        public IActionResult Login(string Email, string Sifre)
        {
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Sifre))
            {
                TempData["ErrorMessage"] = "E-posta ve şifre gereklidir!";
                return View();
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == Email && u.Sifre == Sifre);
            if (user == null)
            {
                TempData["ErrorMessage"] = "E-posta veya şifre hatalı!";
                return View();
            }

            if (user.Role != "Admin")
            {
                TempData["ErrorMessage"] = "Bu sayfaya erişim yetkiniz yok!";
                return View();
            }

            HttpContext.Session.SetString("UserId", user.Id.ToString());
            HttpContext.Session.SetString("UserName", user.KullaniciAdi);
            HttpContext.Session.SetString("UserRole", user.Role);

            return RedirectToAction("Dashboard");
        }

        // GET: Admin/Dashboard
        public IActionResult Dashboard()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login");
            }

            // İstatistikleri hesapla
            var totalMovies = _context.Movies.Count();
            var totalUsers = _context.Users.Count();
            var totalViews = _context.Movies.Sum(m => m.IzlenmeSayisi);
            var avgRating = _context.Movies.Average(m => m.Puan);

            ViewBag.TotalMovies = totalMovies;
            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalViews = totalViews;
            ViewBag.AvgRating = avgRating.ToString("F1");

            // Son eklenen filmler
            var recentMovies = _context.Movies.OrderByDescending(m => m.Id).Take(5).ToList();
            ViewBag.RecentMovies = recentMovies;

            return View();
        }

        // GET: Admin/ManageMovies
        public IActionResult ManageMovies()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login");
            }

            var movies = _context.Movies.OrderByDescending(m => m.Id).ToList();
            return View(movies);
        }

        // GET: Admin/AddMovie
        public IActionResult AddMovie()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login");
            }

            return View();
        }

        // POST: Admin/AddMovie
        [HttpPost]
        public IActionResult AddMovie(MovieModel movie)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login");
            }

            if (string.IsNullOrEmpty(movie.Ad))
            {
                TempData["ErrorMessage"] = "Film adı gereklidir!";
                return View(movie);
            }

            if (string.IsNullOrEmpty(movie.Tür))
            {
                TempData["ErrorMessage"] = "Film türü gereklidir!";
                return View(movie);
            }

            if (movie.Yil <= 0)
            {
                TempData["ErrorMessage"] = "Geçerli bir yıl giriniz!";
                return View(movie);
            }

            // Varsayılan değerler
            if (movie.IzlenmeSayisi == 0) movie.IzlenmeSayisi = 0;
            if (movie.Puan == 0) movie.Puan = 0;

            _context.Movies.Add(movie);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Film başarıyla eklendi!";
            return RedirectToAction("ManageMovies");
        }

        // GET: Admin/EditMovie/{id}
        public IActionResult EditMovie(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login");
            }

            var movie = _context.Movies.FirstOrDefault(m => m.Id == id);
            if (movie == null)
            {
                TempData["ErrorMessage"] = "Film bulunamadı!";
                return RedirectToAction("ManageMovies");
            }

            return View(movie);
        }

        // POST: Admin/EditMovie
        [HttpPost]
        public IActionResult EditMovie(MovieModel movie)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login");
            }

            if (string.IsNullOrEmpty(movie.Ad))
            {
                TempData["ErrorMessage"] = "Film adı gereklidir!";
                return View(movie);
            }

            var existingMovie = _context.Movies.FirstOrDefault(m => m.Id == movie.Id);
            if (existingMovie == null)
            {
                TempData["ErrorMessage"] = "Film bulunamadı!";
                return RedirectToAction("ManageMovies");
            }

            // Film bilgilerini güncelle
            existingMovie.Ad = movie.Ad;
            existingMovie.Tür = movie.Tür;
            existingMovie.Yil = movie.Yil;
            existingMovie.Yönetmen = movie.Yönetmen;
            existingMovie.Oyuncular = movie.Oyuncular;
            existingMovie.Ülke = movie.Ülke;
            existingMovie.Dil = movie.Dil;
            existingMovie.Bütçe = movie.Bütçe;
            existingMovie.GiseHasilati = movie.GiseHasilati;
            existingMovie.IzlenmeSayisi = movie.IzlenmeSayisi;
            existingMovie.Puan = movie.Puan;
            existingMovie.VizyonTarihi = movie.VizyonTarihi;
            existingMovie.Süre = movie.Süre;
            existingMovie.Açıklama = movie.Açıklama;
            existingMovie.AfişUrl = movie.AfişUrl;
            existingMovie.FragmanUrl = movie.FragmanUrl;

            _context.SaveChanges();

            TempData["SuccessMessage"] = "Film başarıyla güncellendi!";
            return RedirectToAction("ManageMovies");
        }

        // POST: Admin/DeleteMovie/{id}
        [HttpPost]
        public IActionResult DeleteMovie(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login");
            }

            var movie = _context.Movies.FirstOrDefault(m => m.Id == id);
            if (movie == null)
            {
                TempData["ErrorMessage"] = "Film bulunamadı!";
                return RedirectToAction("ManageMovies");
            }

            _context.Movies.Remove(movie);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Film başarıyla silindi!";
            return RedirectToAction("ManageMovies");
        }

        // GET: Admin/SyncMovies
        public async Task<IActionResult> SyncMovies()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login");
            }

            var result = await _movieApiService.SyncMoviesToDatabaseAsync(_context);
            
            if (result)
            {
                TempData["SuccessMessage"] = "Filmler başarıyla senkronize edildi!";
            }
            else
            {
                TempData["ErrorMessage"] = "Film senkronizasyonu sırasında hata oluştu!";
            }

            return RedirectToAction("ManageMovies");
        }

        // GET: Admin/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // GET: Admin/CreateAdmin
        public IActionResult CreateAdmin()
        {
            return View();
        }

        // POST: Admin/CreateAdmin
        [HttpPost]
        public IActionResult CreateAdmin(UserModel model, string SifreTekrar)
        {
            if (string.IsNullOrEmpty(model.KullaniciAdi) || string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Sifre))
            {
                TempData["ErrorMessage"] = "Tüm alanlar gereklidir!";
                return View(model);
            }

            if (model.Sifre != SifreTekrar)
            {
                TempData["ErrorMessage"] = "Şifreler eşleşmiyor!";
                return View(model);
            }

            if (model.Sifre.Length < 8)
            {
                TempData["ErrorMessage"] = "Şifre en az 8 karakter olmalıdır!";
                return View(model);
            }

            // E-posta kontrolü
            var existingUser = _context.Users.FirstOrDefault(u => u.Email == model.Email);
            if (existingUser != null)
            {
                TempData["ErrorMessage"] = "Bu e-posta adresi zaten kullanılıyor!";
                return View(model);
            }

            model.KayitTarihi = DateTime.Now;
            model.Role = "Admin"; // Admin rolü

            _context.Users.Add(model);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "🎉 Admin hesabı başarıyla oluşturuldu! Şimdi giriş yapabilirsiniz.";
            return RedirectToAction("Login");
        }

        // GET: Admin/MakeUserAdmin
        public IActionResult MakeUserAdmin()
        {
            var users = _context.Users.Where(u => u.Role != "Admin").ToList();
            return View(users);
        }

        // POST: Admin/MakeUserAdmin
        [HttpPost]
        public IActionResult MakeUserAdmin(int userId)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user != null)
            {
                user.Role = "Admin";
                _context.SaveChanges();
                TempData["SuccessMessage"] = $"Kullanıcı '{user.KullaniciAdi}' admin yapıldı!";
            }
            return RedirectToAction("MakeUserAdmin");
        }
    }
} 