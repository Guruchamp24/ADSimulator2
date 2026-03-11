using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using ADSimulator.Data;

namespace ADSimulator.Models
{
    public static class PasswordHelper
    {
        public static string Hash(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password + "AD_SALT_2024"));
            return Convert.ToBase64String(bytes);
        }
        public static bool Verify(string password, string hash) => Hash(password) == hash;
    }

    public class OUViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "OU Name is required")]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;
        [StringLength(300)]
        public string Description { get; set; } = string.Empty;
    }

    public class CreateUserViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be 3-50 characters")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full Name is required")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm password")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select an OU")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid OU")]
        public int OUId { get; set; }

        public bool IsEnabled { get; set; } = true;
        public List<OrganizationalUnit> OUs { get; set; } = new();
    }

    public class LoginSimulatorViewModel
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; } = string.Empty;
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;
        public string? ResultMessage { get; set; }
        public string? ResultType { get; set; }
        public string? ResultDetail { get; set; }
        public ADUser? AuthenticatedUser { get; set; }
    }

    public class DashboardViewModel
    {
        public int TotalOUs { get; set; }
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int LockedUsers { get; set; }
        public int DisabledUsers { get; set; }
        public List<OrganizationalUnit> OUs { get; set; } = new();
        public List<ADUser> RecentUsers { get; set; } = new();
    }
}
