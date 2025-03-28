using KlippCoApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace KlippCoApp.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

public DbSet<KlippCoApp.Models.Service> Service { get; set; } = default!;

public DbSet<KlippCoApp.Models.StylistSchedule> StylistSchedule { get; set; } = default!;

public DbSet<KlippCoApp.Models.Booking> Bookings { get; set; } = default!;
}
