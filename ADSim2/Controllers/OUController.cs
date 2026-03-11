using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ADSimulator.Data;
using ADSimulator.Models;

namespace ADSimulator.Controllers
{
    public class OUController : Controller
    {
        private readonly AppDbContext _db;
        public OUController(AppDbContext db) { _db = db; }

        public async Task<IActionResult> Index()
        {
            var ous = await _db.OrganizationalUnits.Include(o => o.Users).OrderBy(o => o.Name).ToListAsync();
            return View(ous);
        }

        public IActionResult Create() => View(new OUViewModel());

        [HttpPost][ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OUViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            if (await _db.OrganizationalUnits.AnyAsync(o => o.Name == vm.Name))
            { ModelState.AddModelError("Name", "OU name already exists."); return View(vm); }
            _db.OrganizationalUnits.Add(new OrganizationalUnit { Name = vm.Name, Description = vm.Description });
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Organizational Unit '{vm.Name}' created successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var ou = await _db.OrganizationalUnits.FindAsync(id);
            if (ou == null) return NotFound();
            return View(new OUViewModel { Id = ou.Id, Name = ou.Name, Description = ou.Description });
        }

        [HttpPost][ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(OUViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            var ou = await _db.OrganizationalUnits.FindAsync(vm.Id);
            if (ou == null) return NotFound();
            ou.Name = vm.Name; ou.Description = vm.Description;
            await _db.SaveChangesAsync();
            TempData["Success"] = $"OU '{ou.Name}' updated.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var ou = await _db.OrganizationalUnits.Include(o => o.Users).FirstOrDefaultAsync(o => o.Id == id);
            if (ou == null) return NotFound();
            return View(ou);
        }

        [HttpPost][ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var ou = await _db.OrganizationalUnits.FindAsync(id);
            if (ou == null) return NotFound();
            _db.OrganizationalUnits.Remove(ou);
            await _db.SaveChangesAsync();
            TempData["Success"] = $"OU deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
