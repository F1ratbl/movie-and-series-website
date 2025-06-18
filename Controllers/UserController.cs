using FilmDiziSitesi.Services;
using Microsoft.AspNetCore.Mvc;
using FilmDiziSitesi.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using System;

namespace FilmDiziSitesi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        public static List<UserModel> users = new List<UserModel>();
        private static int nextId = 1;
        private readonly IConfiguration _configuration;

        // IConfiguration'u constructor ile alıyoruz
        public UsersController(IConfiguration configuration)
        {
            _configuration = configuration;
            // TokenService statik yapıysa burada konfigürasyonu veriyoruz
            TokenService.Configure(_configuration);
        }

        [HttpGet]
        public ActionResult<IEnumerable<UserModel>> Get()
        {
            return Ok(users);
        }

        [HttpGet("{id}")]
        public ActionResult<UserModel> Get(int id)
        {
            var user = users.FirstOrDefault(u => u.Id == id);
            if (user == null)
                return NotFound();
            return Ok(user);
        }

        [HttpPost]
        public ActionResult<UserModel> Post([FromBody] UserModel user)
        {
            user.Id = nextId++;
            user.KayitTarihi = DateTime.Now;
            users.Add(user);
            return CreatedAtAction(nameof(Get), new { id = user.Id }, user);
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] UserModel updatedUser)
        {
            var user = users.FirstOrDefault(u => u.Id == id);
            if (user == null)
                return NotFound();

            user.KullaniciAdi = updatedUser.KullaniciAdi;
            user.Email = updatedUser.Email;
            user.Sifre = updatedUser.Sifre;

            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var user = users.FirstOrDefault(u => u.Id == id);
            if (user == null)
                return NotFound();

            users.Remove(user);
            return NoContent();
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] UserModel loginUser)
        {
            var user = users.FirstOrDefault(u =>
                u.Email == loginUser.Email && u.Sifre == loginUser.Sifre);

            if (user == null)
                return Unauthorized("Kullanıcı bulunamadı veya şifre yanlış");

            // TokenService statik ve Configure edilmiş olduğundan doğrudan çağırıyoruz
            var token = TokenService.CreateToken(user.KullaniciAdi);

            return Ok(new { token });
        }
    }
}


