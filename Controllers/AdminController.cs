using Microsoft.AspNetCore.Mvc;
using FilmDiziSitesi.Models;
using FilmDiziSitesi.Data;
using FilmDiziSitesi.Services;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

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

        private bool IsLoggedIn()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("Role");
            return !string.IsNullOrEmpty(userId) && (role == "Admin");
        }

        // GET: /Admin/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login");

            ViewBag.TotalMovies = await _context.Movies.CountAsync();
            ViewBag.TotalUsers = await _context.Users.CountAsync();
            if (await _context.Movies.AnyAsync())
            {
                ViewBag.AvgRating = (await _context.Movies.AverageAsync(m => m.Puan)).ToString("F1");
            }
            else
            {
                ViewBag.AvgRating = "N/A";
            }

            return View();
        }

        // GET: /Admin/ManageMovies
        public async Task<IActionResult> ManageMovies()
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login");
            var movies = await _context.Movies.ToListAsync();
            return View(movies);
        }

        // GET: /Admin/AddMovie
        public IActionResult AddMovie()
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login");
            return View();
        }

        // POST: /Admin/AddMovie
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMovie(MovieModel movie)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login");

            // Otomatik veri düzeltme
            AutoCorrectMovieFields(movie);

            if (ModelState.IsValid)
            {
                _context.Add(movie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ManageMovies));
            }
            return View(movie);
        }

        // GET: /Admin/EditMovie/5
        public async Task<IActionResult> EditMovie(int? id)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login");
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }
            return View(movie);
        }

        // POST: /Admin/EditMovie/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMovie(int id, MovieModel movie)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login");
            if (id != movie.Id)
            {
                return NotFound();
            }

            // Otomatik veri düzeltme
            AutoCorrectMovieFields(movie);

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(movie);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(movie.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(ManageMovies));
            }
            return View(movie);
        }

        // POST: /Admin/DeleteMovie/5
        [HttpPost, ActionName("DeleteMovie")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMovieConfirmed(int id)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login");
            var movie = await _context.Movies.FindAsync(id);
            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ManageMovies));
        }

        // GET: /Admin/ManageUsers
        public async Task<IActionResult> ManageUsers()
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login");
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        // POST: /Admin/DeleteUser/5
        [HttpPost, ActionName("DeleteUser")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login");
            var userToDelete = await _context.Users.FindAsync(id);
            if (userToDelete == null)
            {
                return NotFound();
            }
            var currentUserId = HttpContext.Session.GetString("UserId");
            if (userToDelete.Id.ToString() == currentUserId)
            {
                TempData["ErrorMessage"] = "Kendinizi silemezsiniz.";
                return RedirectToAction(nameof(ManageUsers));
            }
            _context.Users.Remove(userToDelete);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Kullanıcı başarıyla silindi.";
            return RedirectToAction(nameof(ManageUsers));
        }

        // GET: Admin/SyncMovies
        public async Task<IActionResult> SyncMovies()
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login");

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
            return RedirectToAction("Login", "User");
        }

        // GET: /Admin/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Admin/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string email, string sifre)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email && u.Sifre == sifre && u.Role == "Admin");
            if (user != null)
            {
                HttpContext.Session.SetString("UserId", user.Id.ToString());
                HttpContext.Session.SetString("UserName", user.KullaniciAdi);
                HttpContext.Session.SetString("Role", user.Role ?? "User");
                return RedirectToAction("Dashboard");
            }
            ViewBag.Error = "E-posta veya şifre hatalı!";
            return View();
        }

        // GET: /Admin/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Admin/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(UserModel model, string SifreTekrar)
        {
            if (string.IsNullOrEmpty(model.KullaniciAdi))
            {
                ViewBag.Error = "Kullanıcı adı gereklidir!";
                return View(model);
            }
            if (string.IsNullOrEmpty(model.Email))
            {
                ViewBag.Error = "E-posta gereklidir!";
                return View(model);
            }
            if (string.IsNullOrEmpty(model.Sifre))
            {
                ViewBag.Error = "Şifre gereklidir!";
                return View(model);
            }
            if (model.Sifre != SifreTekrar)
            {
                ViewBag.Error = "Şifreler eşleşmiyor!";
                return View(model);
            }
            var existingUser = _context.Users.FirstOrDefault(u => u.Email == model.Email);
            if (existingUser != null)
            {
                ViewBag.Error = "Bu e-posta zaten kayıtlı!";
                return View(model);
            }
            model.Role = "Admin";
            model.KayitTarihi = DateTime.Now;
            _context.Users.Add(model);
            await _context.SaveChangesAsync();
            ViewBag.Success = "Admin hesabı başarıyla oluşturuldu! Giriş yapabilirsiniz.";
            return RedirectToAction("Login");
        }

        private bool MovieExists(int id)
        {
            return _context.Movies.Any(e => e.Id == id);
        }

        // Yardımcı metot: Otomatik veri düzeltme
        private void AutoCorrectMovieFields(MovieModel movie)
        {
            // Ülke ismi dil alanına yazılmışsa düzelt
            var knownCountries = new[] { "Amerika", "Türkiye", "İngiltere", "Fransa", "Almanya", "Japonya", "İtalya", "İspanya", "Rusya", "Çin", "Hindistan", "Kanada", "Brezilya", "Meksika", "Avustralya" };
            if (!string.IsNullOrWhiteSpace(movie.Dil) && knownCountries.Contains(movie.Dil.Trim(), StringComparer.OrdinalIgnoreCase))
            {
                movie.Ülke = movie.Dil;
                movie.Dil = null;
            }
            // Dil ismi ülke alanına yazılmışsa düzelt
            var knownLanguages = new[] { "Türkçe", "İngilizce", "Fransızca", "Almanca", "Japonca", "İtalyanca", "İspanyolca", "Rusça", "Çince", "Hintçe", "Portekizce", "Arapça", "Korece" };
            if (!string.IsNullOrWhiteSpace(movie.Ülke) && knownLanguages.Contains(movie.Ülke.Trim(), StringComparer.OrdinalIgnoreCase))
            {
                movie.Dil = movie.Ülke;
                movie.Ülke = null;
            }
            // Oyuncular alanında tek bir isim varsa ve yönetmen alanı boşsa, bu ismi yönetmen alanına taşı
            if (string.IsNullOrWhiteSpace(movie.Yönetmen) && !string.IsNullOrWhiteSpace(movie.Oyuncular))
            {
                var oyuncularTrim = movie.Oyuncular.Trim();
                // Virgül yoksa ve 4 kelimeden azsa, muhtemelen yönetmen ismi
                if (!oyuncularTrim.Contains(",") && oyuncularTrim.Split(' ').Length <= 4)
                {
                    movie.Yönetmen = oyuncularTrim;
                    movie.Oyuncular = null;
                }
            }
            // Yönetmen alanında birden fazla isim ve virgül varsa, oyuncular alanına taşı (ama oyuncular alanı boşsa)
            if (string.IsNullOrWhiteSpace(movie.Oyuncular) && !string.IsNullOrWhiteSpace(movie.Yönetmen))
            {
                if (movie.Yönetmen.Contains(","))
                {
                    movie.Oyuncular = movie.Yönetmen;
                    movie.Yönetmen = null;
                }
            }
            // Bütçe ve Gişe alanları yanlışlıkla birbirine yazılmışsa düzelt
            if (movie.Bütçe.HasValue && movie.Bütçe > 1000000000 && (!movie.GiseHasilati.HasValue || movie.GiseHasilati == 0))
            {
                movie.GiseHasilati = movie.Bütçe;
                movie.Bütçe = null;
            }
            if (movie.GiseHasilati.HasValue && movie.GiseHasilati < 10000000 && (!movie.Bütçe.HasValue || movie.Bütçe == 0))
            {
                movie.Bütçe = movie.GiseHasilati;
                movie.GiseHasilati = null;
            }
            // Süre alanı yanlışlıkla yıl veya puan gibi sayısal bir değer aldıysa düzelt
            if (!string.IsNullOrWhiteSpace(movie.Süre) && int.TryParse(movie.Süre, out int sureInt) && sureInt > 1900 && sureInt < 2100)
            {
                movie.Yil = sureInt;
                movie.Süre = null;
            }
            // Diğer benzer kontroller buraya eklenebilir
        }
    }
} 