using EmployeeWebApp.Data;
using EmployeeWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using BCrypt.Net;

namespace EmployeeWebApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if the email is already registered
                if (_context.Users.Any(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email is already registered.");
                    return View(model);
                }

                string hashedPassword = HashPassword(model.Password);

                var newUser = new User
                {
                    Email = model.Email,
                    Password = hashedPassword
                };

                _context.Users.Add(newUser);
                _context.SaveChanges();

                return RedirectToAction("Login", "Account");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);

                if (user != null && VerifyPassword(model.Password, user.Password))
                {
                    return RedirectToAction("Index", "Home");
                }
                /*else
                {
                    return RedirectToAction("Privacy", "Home");
                }*/

                ModelState.AddModelError("Credentials", "Invalid email or password.");
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult Logout()
        {
            return RedirectToAction("Login", "Account");
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            string hashedInput = HashPassword(password);

            return hashedInput == hashedPassword;
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
