using API.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<AppUser> Users { get; set; }
}
