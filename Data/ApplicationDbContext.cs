using Microsoft.EntityFrameworkCore;
using FilmDiziSitesi.Models;

namespace FilmDiziSitesi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<UserModel> Users { get; set; }
        public DbSet<MovieModel> Movies { get; set; }
    }
}
