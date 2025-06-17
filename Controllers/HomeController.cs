using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using FilmDiziSitesi.Models;
using FilmDiziSitesi.Services;
using FilmDiziSitesi.Data;

namespace FilmDiziSitesi.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IMovieApiService _movieApiService;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, IMovieApiService movieApiService, ApplicationDbContext context)
    {
        _logger = logger;
        _movieApiService = movieApiService;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            // Veritabanından film sayılarını al
            var movieCount = _context.Movies.Count();
            var topRatedMovies = _context.Movies
                .OrderByDescending(m => m.Puan)
                .Take(8)
                .ToList();
            
            var latestMovies = _context.Movies
                .OrderByDescending(m => m.VizyonTarihi)
                .Take(6)
                .ToList();

            ViewBag.MovieCount = movieCount;
            ViewBag.TopRatedMovies = topRatedMovies;
            ViewBag.LatestMovies = latestMovies;

            // Kategorileri MovieController'dan al
            ViewBag.AllCategories = FilmDiziSitesi.Controllers.MovieController.TumKategoriler;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ana sayfa verileri yüklenirken hata oluştu");
            ViewBag.MovieCount = 0;
            ViewBag.TopRatedMovies = new List<MovieModel>();
            ViewBag.LatestMovies = new List<MovieModel>();
            ViewBag.AllCategories = new List<string>();
        }

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    // API'den film verilerini senkronize et
    public async Task<IActionResult> SyncMovies()
    {
        try
        {
            var success = await _movieApiService.SyncMoviesToDatabaseAsync(_context);
            
            if (success)
            {
                TempData["SuccessMessage"] = "Film verileri başarıyla API'den senkronize edildi!";
            }
            else
            {
                TempData["ErrorMessage"] = "Film verileri senkronize edilirken hata oluştu.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Film senkronizasyonu sırasında hata");
            TempData["ErrorMessage"] = "Senkronizasyon sırasında bir hata oluştu: " + ex.Message;
        }

        return RedirectToAction("Index");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
