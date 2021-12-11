using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sporting_Events.Models;
using Sporting_Events.ViewModels;
using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Sporting_Events.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Register()
        {
            ViewBag.Roles = await _context.Roles.ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                Account account = await _context.Accounts.FirstOrDefaultAsync(u => u.Login == model.Login);

                if (account == null)
                {
                    account = new Account
                    {
                        Login = model.Login,
                        Password = Encoding.UTF8.GetString(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(model.Password))),
                        Name = model.Name,
                        LastName = model.LastName,
                        MiddleName = model.MiddleName,
                        Age = model.Age,
                        Phone = model.Phone,
                        RoleId = model.RoleId
                    };
                    await _context.Accounts.AddAsync(account);
                    await _context.SaveChangesAsync();
                    await Authenticate(account.Id);
                    return RedirectToAction("Index", "Home");
                }
            }
            else
            {
                ModelState.AddModelError("", "Некорректные данные или аккаунт с таким именем уже существует.");
            }
                
            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                Account account = await _context.Accounts
                    .Include(a => a.Role)
                    .FirstOrDefaultAsync(u => u.Login == model.Login && u.Password == Encoding.UTF8.GetString(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(model.Password))));
                if (account != null)
                {
                    await Authenticate(account.Id);

                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("", "Некорректные логин и(или) пароль");
            }
            return View(model);
        }

        private async Task Authenticate(int accountId)
        {
            Account account = await _context.Accounts.Include(a => a.Role).FirstOrDefaultAsync(a => a.Id == accountId);
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, account.Login),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, account.Role.Name),
                new Claim(ClaimTypes.NameIdentifier, account.Id.ToString())
            };
            ClaimsIdentity id = new(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var accountId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            Account account = await _context.Accounts
                .Include(a => a.Role)
                .Include(a => a.Competitions)
                .FirstOrDefaultAsync(a => a.Id == accountId);

            return View(account);
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Index(int roleId = 0)
        {
            ViewBag.Roles = await _context.Roles.ToListAsync();
            List<Account> accounts = new();
            if (roleId == 0)
            {
                accounts = await _context.Accounts
                    .Include(a => a.Role)
                    .ToListAsync();
                return View(accounts);
            }
            accounts = await _context.Accounts
                .Include(a => a.Role)
                .Where(a => a.Role.Id == roleId)
                .ToListAsync();
            return View(accounts);
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == id);
            if (account == null)
            {
                return NotFound();
            }

            return View(account);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var account = await _context.Accounts.FindAsync(id);
            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
