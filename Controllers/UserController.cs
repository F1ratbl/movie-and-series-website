using Microsoft.AspNetCore.Mvc;
using FilmDiziSitesi.Models;
using FilmDiziSitesi.Data;  // ApplicationDbContext burada
using System.Linq;

namespace FilmDiziSitesi.Controllers
{
    // MVC Controller - Web sayfaları için
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: User/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: User/Login
        [HttpPost]
        public IActionResult Login(UserModel model)
        {
            // Manuel validation
            if (string.IsNullOrEmpty(model.Email))
            {
                TempData["ErrorMessage"] = "E-posta adresi gereklidir!";
                return View(model);
            }

            if (string.IsNullOrEmpty(model.Sifre))
            {
                TempData["ErrorMessage"] = "Şifre gereklidir!";
                return View(model);
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == model.Email && u.Sifre == model.Sifre);
            if (user != null)
            {
                HttpContext.Session.SetString("UserId", user.Id.ToString());
                HttpContext.Session.SetString("UserName", user.KullaniciAdi);
                return RedirectToAction("Index", "Home");
            }
            else
            {
                TempData["ErrorMessage"] = "E-posta veya şifre hatalı!";
            }
            return View(model);
        }

        // GET: User/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: User/Register
        [HttpPost]
        public IActionResult Register(UserModel model, string SifreTekrar)
        {
            // Manuel validation
            if (string.IsNullOrEmpty(model.KullaniciAdi))
            {
                TempData["ErrorMessage"] = "Kullanıcı adı gereklidir!";
                return View(model);
            }

            if (model.KullaniciAdi.Length < 3 || model.KullaniciAdi.Length > 50)
            {
                TempData["ErrorMessage"] = "Kullanıcı adı 3-50 karakter arasında olmalıdır!";
                return View(model);
            }

            if (string.IsNullOrEmpty(model.Email))
            {
                TempData["ErrorMessage"] = "E-posta adresi gereklidir!";
                return View(model);
            }

            if (!model.Email.Contains("@") || !model.Email.Contains("."))
            {
                TempData["ErrorMessage"] = "Geçerli bir e-posta adresi giriniz!";
                return View(model);
            }

            if (string.IsNullOrEmpty(model.Sifre))
            {
                TempData["ErrorMessage"] = "Şifre gereklidir!";
                return View(model);
            }

            if (model.Sifre.Length < 8)
            {
                TempData["ErrorMessage"] = "Şifre en az 8 karakter olmalıdır!";
                return View(model);
            }

            // Şifre tekrarı kontrolü
            if (model.Sifre != SifreTekrar)
            {
                TempData["ErrorMessage"] = "Şifreler eşleşmiyor!";
                return View(model);
            }

            // E-posta kontrolü
            var existingUser = _context.Users.FirstOrDefault(u => u.Email == model.Email);
            if (existingUser != null)
            {
                TempData["ErrorMessage"] = "Bu e-posta adresi zaten kullanılıyor!";
                return View(model);
            }

            // Kullanıcı adı kontrolü
            var existingUsername = _context.Users.FirstOrDefault(u => u.KullaniciAdi == model.KullaniciAdi);
            if (existingUsername != null)
            {
                TempData["ErrorMessage"] = "Bu kullanıcı adı zaten kullanılıyor!";
                return View(model);
            }

            model.KayitTarihi = DateTime.Now;
            model.Role = "User"; // Varsayılan rol

            _context.Users.Add(model);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "🎉 Kayıt başarılı! Şimdi giriş yapabilirsiniz.";
            return RedirectToAction("Login");
        }

        // GET: User/Profile
        public IActionResult Profile()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            var user = _context.Users.FirstOrDefault(u => u.Id == int.Parse(userId));
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            return View(user);
        }

        // GET: User/EditProfile
        public IActionResult EditProfile()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            var user = _context.Users.FirstOrDefault(u => u.Id == int.Parse(userId));
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            return View(user);
        }

        // POST: User/EditProfile
        [HttpPost]
        public IActionResult EditProfile(UserModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _context.Users.FirstOrDefault(u => u.Id == model.Id);
                if (user != null)
                {
                    user.KullaniciAdi = model.KullaniciAdi;
                    user.Email = model.Email;
                    // Şifre değişikliği için ayrı kontrol eklenebilir

                    _context.SaveChanges();
                    TempData["SuccessMessage"] = "Profil başarıyla güncellendi!";
                    return RedirectToAction("Profile");
                }
            }
            return View(model);
        }

        // GET: User/ForgotPassword
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // POST: User/ForgotPassword
        [HttpPost]
        public IActionResult ForgotPassword(string Email)
        {
            if (string.IsNullOrEmpty(Email))
            {
                TempData["ErrorMessage"] = "E-posta adresi gereklidir.";
                return View();
            }

            // E-posta kontrolü
            var user = _context.Users.FirstOrDefault(u => u.Email == Email);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Bu e-posta adresi sistemde kayıtlı değil.";
                return View();
            }

            // Şifre sıfırlama işlemi burada yapılacak
            // Gerçek uygulamada e-posta gönderme işlemi eklenir
            TempData["SuccessMessage"] = "Şifre sıfırlama bağlantısı e-posta adresinize gönderildi.";
            return RedirectToAction("Login");
        }

        // GET: User/ResetPassword
        public IActionResult ResetPassword()
        {
            return View();
        }

        // POST: User/ResetPassword
        [HttpPost]
        public IActionResult ResetPassword(string Token, string NewPassword, string ConfirmPassword)
        {
            if (string.IsNullOrEmpty(NewPassword))
            {
                TempData["ErrorMessage"] = "Yeni şifre gereklidir.";
                return View();
            }

            if (NewPassword != ConfirmPassword)
            {
                TempData["ErrorMessage"] = "Şifreler eşleşmiyor.";
                return View();
            }

            if (NewPassword.Length < 8)
            {
                TempData["ErrorMessage"] = "Şifre en az 8 karakter olmalıdır.";
                return View();
            }

            // Şifre sıfırlama işlemi burada yapılacak
            // Gerçek uygulamada token doğrulama ve şifre güncelleme işlemi eklenir
            TempData["SuccessMessage"] = "Şifreniz başarıyla sıfırlandı!";
            return RedirectToAction("Login");
        }

        // GET: User/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }

    // API Controller - API endpoint'leri için
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        // DbContext'i constructor ile alıyoruz (dependency injection)
        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/users
        [HttpGet]
        public IActionResult Get()
        {       
            var users = _context.Users.ToList();
            return Ok(users);
        }

        // GET: api/users/{id}
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
                return NotFound();

            return Ok(user);
        }
        
        // POST: api/users
        [HttpPost]
        public IActionResult Post([FromBody] UserModel user)
        {
            if (user == null)
                return BadRequest();

            // Kaydetme tarihini güncelle (isteğe bağlı)
            user.KayitTarihi = DateTime.Now;

            _context.Users.Add(user);
            _context.SaveChanges();

            return CreatedAtAction(nameof(Get), new { id = user.Id }, user);
        }

        // PUT: api/users/{id}
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] UserModel updatedUser)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
                return NotFound();

            user.KullaniciAdi = updatedUser.KullaniciAdi;
            user.Email = updatedUser.Email;
            user.Sifre = updatedUser.Sifre;
            user.Role = updatedUser.Role;

            _context.SaveChanges();

            return NoContent();
        }

        // DELETE: api/users/{id}
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
                return NotFound();

            _context.Users.Remove(user);
            _context.SaveChanges();

            return NoContent();
        }
    }
}
