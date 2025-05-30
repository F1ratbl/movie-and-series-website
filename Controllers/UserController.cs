using Microsoft.AspNetCore.Mvc;
using FilmDiziSitesi.Models;
using System.Collections.Generic;
using System.Linq;

namespace FilmDiziSitesi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private static List<UserModel> users = new List<UserModel>();
        private static int nextId = 1;

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
    }
}
