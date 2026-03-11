using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ADSimulator.Data;
using ADSimulator.Models;

namespace ADSimulator.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _db;
        private const int MaxAttempts = 3;
        public AuthController(AppDbContext db) { _db = db; }

        public IActionResult LoginSimulator() => View(new LoginSimulatorViewModel());

        [HttpPost][ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginSimulator(LoginSimulatorViewModel vm)
        {
            vm.HasResult = true;
            if (!ModelState.IsValid) { vm.ResultType = "error"; vm.ResultMessage = "Please fill all fields."; return View(vm); }

            var user = await _db.Users.Include(u => u.OU).FirstOrDefaultAsync(u => u.Username == vm.Username);

            if (user == null)
            {
                vm.ResultType = "error";
                vm.ResultMessage = "Authentication Failed";
                vm.ResultDetail = $"The user account '{vm.Username}' was not found in the Active Directory.";
                return View(vm);
            }

            if (!user.IsEnabled)
            {
                vm.ResultType = "disabled";
                vm.ResultMessage = "Account Disabled";
                vm.ResultDetail = $"The account '{vm.Username}' has been disabled by an administrator. Contact your system administrator.";
                return View(vm);
            }

            if (user.IsLocked)
            {
                vm.ResultType = "locked";
                vm.ResultMessage = "Account Locked Out";
                vm.ResultDetail = $"This account has been locked after {MaxAttempts} failed logon attempts. An administrator must unlock the account.";
                return View(vm);
            }

            if (!PasswordHelper.Verify(vm.Password, user.PasswordHash))
            {
                user.FailedLoginAttempts++;
                if (user.FailedLoginAttempts >= MaxAttempts)
                {
                    user.IsLocked = true;
                    await _db.SaveChangesAsync();
                    vm.ResultType = "locked";
                    vm.ResultMessage = "Account Now Locked";
                    vm.ResultDetail = $"Account locked after {MaxAttempts} failed attempts. Contact your administrator to unlock.";
                }
                else
                {
                    await _db.SaveChangesAsync();
                    int remaining = MaxAttempts - user.FailedLoginAttempts;
                    vm.ResultType = "error";
                    vm.ResultMessage = "Invalid Credentials";
                    vm.ResultDetail = $"The username or password is incorrect. {remaining} attempt(s) remaining before lockout.";
                }
                return View(vm);
            }

            user.FailedLoginAttempts = 0;
            user.LastLogin = DateTime.Now;
            await _db.SaveChangesAsync();

            vm.ResultType = "success";
            vm.ResultMessage = "Logon Successful";
            vm.ResultDetail = $"Welcome, {user.FullName}. You have been authenticated successfully.";
            vm.AuthenticatedUser = user;
            return View(vm);
        }
    }
}
