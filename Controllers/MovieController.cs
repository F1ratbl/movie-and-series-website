using Microsoft.AspNetCore.Mvc;
using FilmDiziSitesi.Models;
using FilmDiziSitesi.Data;
using System.Collections.Generic;
using System.Linq;

namespace FilmDiziSitesi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MoviesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/movies
        [HttpGet]
        public ActionResult<IEnumerable<MovieModel>> Get()
        {
            var movies = _context.Movies.ToList();
            return Ok(movies);
        }

        // GET: api/movies/5
        [HttpGet("{id}")]
        public ActionResult<MovieModel> Get(int id)
        {
            var movie = _context.Movies.Find(id);
            if (movie == null)
                return NotFound();

            return Ok(movie);
        }

        // POST: api/movies
        [HttpPost]
        public ActionResult<MovieModel> Post([FromBody] MovieModel movie)
        {
            _context.Movies.Add(movie);
            _context.SaveChanges();

            return CreatedAtAction(nameof(Get), new { id = movie.Id }, movie);
        }

        // PUT: api/movies/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] MovieModel updatedMovie)
        {
            var movie = _context.Movies.Find(id);
            if (movie == null)
                return NotFound();

            _context.Entry(movie).CurrentValues.SetValues(updatedMovie);
            _context.SaveChanges();

            return NoContent();
        }

        // DELETE: api/movies/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var movie = _context.Movies.Find(id);
            if (movie == null)
                return NotFound();

            _context.Movies.Remove(movie);
            _context.SaveChanges();

            return NoContent();
        }
    }
}
