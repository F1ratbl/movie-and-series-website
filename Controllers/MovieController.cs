using Microsoft.AspNetCore.Mvc;
using FilmDiziSitesi.Models;
using FilmDiziSitesi.Data;
using System.Collections.Generic;
using System.Linq;

namespace FilmDiziSitesi.Controllers
{
    // API controller olduğunu ve route olarak "api/movies" kullanıldığını belirtir
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        // Veritabanı işlemleri için context nesnesi (Dependency Injection ile sağlanır)
        private readonly ApplicationDbContext _context;

        // Constructor: context'i alıp sınıf değişkenine atar
        public MoviesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/movies
        // Tüm filmleri listelemek için endpoint
        [HttpGet]
        public ActionResult<IEnumerable<MovieModel>> Get()
        {
            // Veritabanından tüm filmleri çek
            var movies = _context.Movies.ToList();

            // HTTP 200 OK ile filmleri döndür
            return Ok(movies);
        }

        // GET: api/movies/5
        // Belirli bir id'ye sahip filmi getirir
        [HttpGet("{id}")]
        public ActionResult<MovieModel> Get(int id)
        {
            // Verilen id ile filmi bul
            var movie = _context.Movies.Find(id);

            // Eğer film yoksa 404 Not Found döndür
            if (movie == null)
                return NotFound();

            // Film varsa 200 OK ile döndür
            return Ok(movie);
        }

        // POST: api/movies
        // Yeni film eklemek için endpoint
        [HttpPost]
        public ActionResult<MovieModel> Post([FromBody] MovieModel movie)
        {
            // Yeni filmi veritabanına ekle
            _context.Movies.Add(movie);
            // Değişiklikleri kaydet (INSERT işlemi)
            _context.SaveChanges();

            // Oluşturulan kaynağın adresi ve verisi ile 201 Created döndür
            return CreatedAtAction(nameof(Get), new { id = movie.Id }, movie);
        }

        // PUT: api/movies/5
        // Var olan filmi güncellemek için endpoint
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] MovieModel updatedMovie)
        {
            // Güncellenecek filmi id ile bul
            var movie = _context.Movies.Find(id);

            // Film yoksa 404 döndür
            if (movie == null)
                return NotFound();

            // Veritabanındaki film nesnesinin tüm alanlarını gelen updatedMovie ile değiştir
            _context.Entry(movie).CurrentValues.SetValues(updatedMovie);

            // Değişiklikleri kaydet (UPDATE işlemi)
            _context.SaveChanges();

            // Başarılı ve içeriksiz cevap (HTTP 204 No Content)
            return NoContent();
        }

        // DELETE: api/movies/5
        // Belirtilen id'li filmi silmek için endpoint
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            // Silinecek filmi bul
            var movie = _context.Movies.Find(id);

            // Film yoksa 404 döndür
            if (movie == null)
                return NotFound();

            // Filmi veritabanından kaldır
            _context.Movies.Remove(movie);

            // Değişiklikleri kaydet (DELETE işlemi)
            _context.SaveChanges();

            // Başarılı ve içeriksiz cevap (HTTP 204 No Content)
            return NoContent();
        }
    }
}
