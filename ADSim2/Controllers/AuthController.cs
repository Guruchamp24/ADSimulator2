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
            if (!ModelState.IsValid) return View(vm);

            var user = await _db.Users.Include(u => u.OU).FirstOrDefaultAsync(u => u.Username == vm.Username);

            if (user == null)
            {
                vm.ResultType = "fail";
                vm.ResultMessage = "Authentication Failed";
                vm.ResultDetail = $"The user account '{vm.Username}' does not exist in this domain.";
                return View(vm);
            }
            if (!user.IsEnabled)
            {
                vm.ResultType = "disabled";
                vm.ResultMessage = "Account Disabled";
                vm.ResultDetail = $"The account '{vm.Username}' has been disabled by an administrator. Contact your domain administrator.";
                return View(vm);
            }
            if (user.IsLocked)
            {
                vm.ResultType = "locked";
                vm.ResultMessage = "Account Locked Out";
                vm.ResultDetail = $"This account has been locked due to {MaxAttempts} failed sign-in attempts. Contact your domain administrator to unlock.";
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
                    vm.ResultMessage = "Account Locked Out";
                    vm.ResultDetail = $"Account locked after {MaxAttempts} failed attempts. Contact your domain administrator.";
                }
                else
                {
                    await _db.SaveChangesAsync();
                    int left = MaxAttempts - user.FailedLoginAttempts;
                    vm.ResultType = "fail";
                    vm.ResultMessage = "Authentication Failed";
                    vm.ResultDetail = $"The password is incorrect. {left} attempt(s) remaining before lockout. (Attempt {user.FailedLoginAttempts}/{MaxAttempts})";
                }
                return View(vm);
            }

            user.FailedLoginAttempts = 0;
            user.LastLogin = DateTime.Now;
            await _db.SaveChangesAsync();
            vm.ResultType = "success";
            vm.ResultMessage = "Authentication Successful";
            vm.ResultDetail = $"Welcome back, {user.FullName}. You have been successfully authenticated.";
            vm.AuthenticatedUser = user;
            return View(vm);
        }
    }
}
