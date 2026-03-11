using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ADSimulator.Data;
using ADSimulator.Models;

namespace ADSimulator.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;
        public HomeController(AppDbContext db) { _db = db; }

        public async Task<IActionResult> Index()
        {
            var users = await _db.Users.Include(u => u.OU).OrderByDescending(u => u.CreatedAt).ToListAsync();
            var ous = await _db.OrganizationalUnits.Include(o => o.Users).ToListAsync();
            return View(new DashboardViewModel
            {
                TotalOUs = ous.Count,
                TotalUsers = users.Count,
                ActiveUsers = users.Count(u => u.IsEnabled && !u.IsLocked),
                LockedUsers = users.Count(u => u.IsLocked),
                DisabledUsers = users.Count(u => !u.IsEnabled),
                OUs = ous,
                RecentUsers = users.Take(10).ToList()
            });
        }
    }
}
