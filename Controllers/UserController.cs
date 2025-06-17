using Microsoft.AspNetCore.Mvc;
using FilmDiziSitesi.Models;
using FilmDiziSitesi.Data;  // ApplicationDbContext burada
using System.Linq;

namespace FilmDiziSitesi.Controllers
{
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
