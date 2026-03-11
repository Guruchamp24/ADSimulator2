using Microsoft.EntityFrameworkCore;

namespace ADSimulator.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<OrganizationalUnit> OrganizationalUnits { get; set; }
        public DbSet<ADUser> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrganizationalUnit>()
                .HasMany(o => o.Users)
                .WithOne(u => u.OU)
                .HasForeignKey(u => u.OUId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class OrganizationalUnit
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public List<ADUser> Users { get; set; } = new();
    }

    public class ADUser
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public bool IsEnabled { get; set; } = true;
        public bool IsLocked { get; set; } = false;
        public int FailedLoginAttempts { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? LastLogin { get; set; }
        public int OUId { get; set; }
        public OrganizationalUnit? OU { get; set; }
    }
}
