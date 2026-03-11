using Microsoft.EntityFrameworkCore;
using ADSimulator.Data;
using ADSimulator.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlite("Data Source=adsimulator.db"));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    if (!db.OrganizationalUnits.Any())
    {
        var it = new OrganizationalUnit { Name = "IT Department", Description = "Information Technology" };
        var hr = new OrganizationalUnit { Name = "HR Department", Description = "Human Resources" };
        var mg = new OrganizationalUnit { Name = "Management", Description = "Management & Executives" };
        db.OrganizationalUnits.AddRange(it, hr, mg);
        db.SaveChanges();
        db.Users.AddRange(
            new ADUser { Username = "admin", FullName = "System Administrator", Email = "admin@company.local", PasswordHash = PasswordHelper.Hash("Admin@123"), OUId = it.Id, IsEnabled = true },
            new ADUser { Username = "jdoe", FullName = "John Doe", Email = "jdoe@company.local", PasswordHash = PasswordHelper.Hash("Pass123"), OUId = hr.Id, IsEnabled = true },
            new ADUser { Username = "locked.user", FullName = "Locked Test User", Email = "locked@company.local", PasswordHash = PasswordHelper.Hash("Test123"), OUId = it.Id, IsEnabled = true, IsLocked = true, FailedLoginAttempts = 3 }
        );
        db.SaveChanges();
    }
}

app.UseStaticFiles();
app.UseRouting();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();
