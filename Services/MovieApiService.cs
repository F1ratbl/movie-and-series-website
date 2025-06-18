using System.Text.Json;
using FilmDiziSitesi.Models;
using FilmDiziSitesi.Data;

namespace FilmDiziSitesi.Services
{
    public interface IMovieApiService
    {
        Task<List<MovieModel>> GetMoviesFromApiAsync();
        Task<bool> SyncMoviesToDatabaseAsync(ApplicationDbContext context);
        Task<Dictionary<int, MovieModel>> GetMoviesByIdAsync();
        Task<MovieModel?> GetMovieByIdAsync(int id);
    }

    public class MovieApiService : IMovieApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<MovieApiService> _logger;

        public MovieApiService(HttpClient httpClient, ILogger<MovieApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<MovieModel>> GetMoviesFromApiAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/movies");
                response.EnsureSuccessStatusCode();

                var jsonString = await response.Content.ReadAsStringAsync();
                
                // JSON deserialization için özel ayarlar
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                
                var movies = JsonSerializer.Deserialize<List<MovieModel>>(jsonString, options);

                _logger.LogInformation($"API'den {movies?.Count ?? 0} film başarıyla çekildi");
                return movies ?? new List<MovieModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API'den film verileri çekilirken hata oluştu");
                return new List<MovieModel>();
            }
        }

        public async Task<bool> SyncMoviesToDatabaseAsync(ApplicationDbContext context)
        {
            try
            {
                var apiMovies = await GetMoviesFromApiAsync();
                if (!apiMovies.Any())
                {
                    _logger.LogWarning("API'den film verisi alınamadı");
                    return false;
                }

                // Mevcut filmleri kontrol et
                var existingMovies = context.Movies.ToList();
                int addedCount = 0;
                int updatedCount = 0;
                
                foreach (var apiMovie in apiMovies)
                {
                    // API'den gelen filmi ID'ye göre kontrol et
                    var existingMovie = existingMovies.FirstOrDefault(m => m.Id == apiMovie.Id);
                    
                    if (existingMovie == null)
                    {
                        // Yeni film ekle - ID'yi API'den gelen ID ile ayarla
                        context.Movies.Add(apiMovie);
                        addedCount++;
                        _logger.LogInformation($"Yeni film eklendi: {apiMovie.Ad} (ID: {apiMovie.Id})");
                    }
                    else
                    {
                        // Mevcut filmi güncelle
                        existingMovie.Ad = apiMovie.Ad;
                        existingMovie.Tür = apiMovie.Tür;
                        existingMovie.Yil = apiMovie.Yil;
                        existingMovie.Yönetmen = apiMovie.Yönetmen;
                        existingMovie.Oyuncular = apiMovie.Oyuncular;
                        existingMovie.Ülke = apiMovie.Ülke;
                        existingMovie.Dil = apiMovie.Dil;
                        existingMovie.Bütçe = apiMovie.Bütçe;
                        existingMovie.GiseHasilati = apiMovie.GiseHasilati;
                        existingMovie.IzlenmeSayisi = apiMovie.IzlenmeSayisi;
                        existingMovie.Puan = apiMovie.Puan;
                        existingMovie.VizyonTarihi = apiMovie.VizyonTarihi;
                        existingMovie.Süre = apiMovie.Süre;
                        existingMovie.Açıklama = apiMovie.Açıklama;
                        existingMovie.AfişUrl = apiMovie.AfişUrl;
                        existingMovie.FragmanUrl = apiMovie.FragmanUrl;
                        
                        updatedCount++;
                        _logger.LogInformation($"Film güncellendi: {apiMovie.Ad} (ID: {apiMovie.Id})");
                    }
                }

                await context.SaveChangesAsync();
                _logger.LogInformation($"Veritabanı senkronizasyonu tamamlandı. {addedCount} yeni film eklendi, {updatedCount} film güncellendi. Toplam {apiMovies.Count} film işlendi");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Veritabanı senkronizasyonu sırasında hata oluştu");
                return false;
            }
        }

        public async Task<Dictionary<int, MovieModel>> GetMoviesByIdAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("http://localhost:5195/api/movies");
                
                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var movies = JsonSerializer.Deserialize<List<MovieModel>>(jsonContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    // ID'ye göre dictionary oluştur
                    var moviesById = movies?.ToDictionary(m => m.Id, m => m) ?? new Dictionary<int, MovieModel>();
                    
                    _logger.LogInformation($"API'den {moviesById.Count} film başarıyla alındı");
                    return moviesById;
                }
                else
                {
                    _logger.LogError($"API'den veri alınamadı. Status: {response.StatusCode}");
                    return new Dictionary<int, MovieModel>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API'den film verileri alınırken hata oluştu");
                return new Dictionary<int, MovieModel>();
            }
        }

        public async Task<MovieModel?> GetMovieByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"http://localhost:5195/api/movies/{id}");
                
                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var movie = JsonSerializer.Deserialize<MovieModel>(jsonContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    
                    _logger.LogInformation($"ID {id} olan film başarıyla alındı");
                    return movie;
                }
                else
                {
                    _logger.LogWarning($"ID {id} olan film bulunamadı. Status: {response.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"ID {id} olan film alınırken hata oluştu");
                return null;
            }
        }
    }
} 