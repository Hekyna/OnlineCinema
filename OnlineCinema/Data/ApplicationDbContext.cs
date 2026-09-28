using Microsoft.EntityFrameworkCore;
using OnlineCinema.Models;

namespace OnlineCinema.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Movie> Movies { get; set; }
}