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

    // MVC Controller: ViewResult döndüren, /Movie/Index gibi istekleri karşılayan controller
    [Route("Movie")]
    public class MovieController : Controller
    {
        private readonly ApplicationDbContext _context;
        // Tüm kategoriler merkezi olarak burada tanımlanıyor
        public static readonly List<string> TumKategoriler = new List<string>
        {
            "Filmler", "Diziler", "Aksiyon", "Komedi", "Romantik", "Gerilim", "Dram", "Korku", "Belgesel", "Animasyon", "Polisiye", "Macera", "Fantastik"
        };
        public MovieController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Movie/Index
        public IActionResult Index(string category, int? year, double? minRating, string sortBy)
        {
            var movies = _context.Movies.AsQueryable();

            // Filtreler
            if (!string.IsNullOrEmpty(category))
                movies = movies.Where(m => m.Tür == category);
            if (year.HasValue)
                movies = movies.Where(m => m.Yil == year.Value);
            if (minRating.HasValue)
                movies = movies.Where(m => m.Puan >= minRating.Value);

            // Sıralama
            switch (sortBy)
            {
                case "year":
                    movies = movies.OrderByDescending(m => m.Yil);
                    break;
                case "title":
                    movies = movies.OrderBy(m => m.Ad);
                    break;
                case "date":
                    movies = movies.OrderByDescending(m => m.VizyonTarihi);
                    break;
                default:
                    movies = movies.OrderByDescending(m => m.Puan);
                    break;
            }

            return View(movies.ToList());
        }

        // GET: /Movie/Category
        [HttpGet("Category")]
        [HttpGet("Category/{category}")]
        public IActionResult Category(string category)
        {
            if (string.IsNullOrEmpty(category))
            {
                return View(_context.Movies.ToList());
            }

            var normalizedCategory = category.Replace("%20", " ").Replace("-", " ").ToLower();

            // "Filmler" ve "Diziler" özel kategorileri
            if (string.Equals(category, "Filmler", StringComparison.OrdinalIgnoreCase))
            {
                return View(_context.Movies.Where(m => !m.Tür.ToLower().Contains("dizi")).ToList());
            }
            if (string.Equals(category, "Diziler", StringComparison.OrdinalIgnoreCase))
            {
                return View(_context.Movies.Where(m => m.Tür.ToLower().Contains("dizi")).ToList());
            }

            // Diğer kategoriler için, birden fazla kategori varsa virgül veya tire ile ayırıp eşleşme yap
            var allMovies = _context.Movies.ToList();
            var movies = allMovies.Where(m => m.Tür != null && m.Tür.ToLower().Split(',', '-', '/').Select(t => t.Trim()).Any(t => t == normalizedCategory)).ToList();
            return View(movies);
        }

        [HttpGet("Search")]
        public IActionResult Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                ViewBag.SearchQuery = "";
                return View(new List<MovieModel>());
            }

            var results = _context.Movies
                .Where(m =>
                    m.Ad.ToLower().Contains(query.ToLower()) ||
                    m.Tür.ToLower().Contains(query.ToLower()) ||
                    m.Oyuncular.ToLower().Contains(query.ToLower()))
                .ToList();

            ViewBag.SearchQuery = query;
            return View(results);
        }

        // Ana sayfa kategorileri için action
        [HttpGet("Categories")]
        public IActionResult Categories()
        {
            ViewBag.AllCategories = TumKategoriler;
            // Sadece ilk 6 kategori ana sayfada gösterilecek
            return View(TumKategoriler.Take(6).ToList());
        }
    }
}
