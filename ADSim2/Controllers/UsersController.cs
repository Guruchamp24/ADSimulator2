using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ADSimulator.Data;
using ADSimulator.Models;

namespace ADSimulator.Controllers
{
    public class UsersController : Controller
    {
        private readonly AppDbContext _db;
        public UsersController(AppDbContext db) { _db = db; }

        public async Task<IActionResult> Index(string? search, int? ouId)
        {
            var q = _db.Users.Include(u => u.OU).AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(u => u.Username.Contains(search) || u.FullName.Contains(search) || u.Email.Contains(search));
            if (ouId.HasValue)
                q = q.Where(u => u.OUId == ouId.Value);

            ViewBag.Search = search;
            ViewBag.OUId = ouId;
            ViewBag.OUs = await _db.OrganizationalUnits.OrderBy(o => o.Name).ToListAsync();
            return View(await q.OrderBy(u => u.Username).ToListAsync());
        }

        public async Task<IActionResult> Create()
        {
            var ous = await _db.OrganizationalUnits.OrderBy(o => o.Name).ToListAsync();
            if (!ous.Any())
            {
                TempData["Warning"] = "Please create an Organizational Unit first.";
                return RedirectToAction("Create", "OU");
            }
            var vm = new CreateUserViewModel { OUs = ous };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel vm)
        {
            // Always reload OUs before returning view
            vm.OUs = await _db.OrganizationalUnits.OrderBy(o => o.Name).ToListAsync();

            if (!ModelState.IsValid)
                return View(vm);

            if (await _db.Users.AnyAsync(u => u.Username == vm.Username))
            {
                ModelState.AddModelError("Username", "This username already exists.");
                return View(vm);
            }

            var user = new ADUser
            {
                Username = vm.Username.Trim(),
                FullName = vm.FullName.Trim(),
                Email = vm.Email.Trim(),
                PasswordHash = PasswordHelper.Hash(vm.Password),
                IsEnabled = vm.IsEnabled,
                OUId = vm.OUId,
                CreatedAt = DateTime.Now
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            TempData["Success"] = $"User '{user.Username}' created successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var user = await _db.Users.Include(u => u.OU).FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();
            return View(user);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();
            var vm = new CreateUserViewModel
            {
                Id = user.Id, Username = user.Username, FullName = user.FullName,
                Email = user.Email, OUId = user.OUId, IsEnabled = user.IsEnabled,
                Password = "UNCHANGED", ConfirmPassword = "UNCHANGED",
                OUs = await _db.OrganizationalUnits.OrderBy(o => o.Name).ToListAsync()
            };
            return View(vm);
        }

        [HttpPost][ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string FullName, string Email, int OUId, bool IsEnabled, string? NewPassword, string? ConfirmPassword)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.FullName = FullName;
            user.Email = Email;
            user.OUId = OUId;
            user.IsEnabled = IsEnabled;

            if (!string.IsNullOrWhiteSpace(NewPassword))
            {
                if (NewPassword.Length < 6)
                { TempData["Error"] = "Password must be at least 6 characters."; return RedirectToAction(nameof(Edit), new { id }); }
                if (NewPassword != ConfirmPassword)
                { TempData["Error"] = "Passwords do not match."; return RedirectToAction(nameof(Edit), new { id }); }
                user.PasswordHash = PasswordHelper.Hash(NewPassword);
            }

            await _db.SaveChangesAsync();
            TempData["Success"] = $"User '{user.Username}' updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost][ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();
            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
            TempData["Success"] = "User deleted.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost][ValidateAntiForgeryToken]
        public async Task<IActionResult> Unlock(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();
            user.IsLocked = false; user.FailedLoginAttempts = 0;
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Account '{user.Username}' unlocked.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost][ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleEnabled(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();
            user.IsEnabled = !user.IsEnabled;
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Account '{user.Username}' {(user.IsEnabled ? "enabled" : "disabled")}.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost][ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(int id, string newPassword, string confirmPassword)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();
            if (newPassword.Length < 6) { TempData["Error"] = "Minimum 6 characters required."; return RedirectToAction(nameof(Details), new { id }); }
            if (newPassword != confirmPassword) { TempData["Error"] = "Passwords do not match."; return RedirectToAction(nameof(Details), new { id }); }
            user.PasswordHash = PasswordHelper.Hash(newPassword);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Password reset successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
