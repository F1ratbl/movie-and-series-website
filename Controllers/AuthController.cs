using Microsoft.AspNetCore.Mvc;
using FilmDiziSitesi.Models;
using FilmDiziSitesi.Services;
using System.Linq;

namespace FilmDiziSitesi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        [HttpPost("login")]
        public IActionResult Login([FromBody] UserModel giris)
        {
            var user = UsersController.users.FirstOrDefault(
                u => u.KullaniciAdi == giris.KullaniciAdi && u.Sifre == giris.Sifre);

            if (user == null)
                return Unauthorized(new { mesaj = "Hatalı kullanıcı adı veya şifre." });

            var token = TokenService.CreateToken(user.KullaniciAdi);
            return Ok(new { token });
        }
    }
}
