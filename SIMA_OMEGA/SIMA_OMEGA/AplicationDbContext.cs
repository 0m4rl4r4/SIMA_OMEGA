using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SIMA_OMEGA.Entities;

namespace SIMA_OMEGA
{
    public class ApplicationUser : IdentityUser
    {
        public string? ProfileImage { get; set; }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<SensorData> Sensores { get; set; }

        public DbSet<Evento> Eventos { get; set; }
    }
}

