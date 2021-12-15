using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sporting_Events.Models;

namespace Sporting_Events.Controllers
{
    public class RequestController : Controller
    {
        private readonly AppDbContext _context;

        public RequestController(AppDbContext context)
        {
            _context = context;
        }
        
        [Authorize(Roles = "sportsman")]
        [HttpPost]
        public async Task<IActionResult> Create(int compId)
        {
            var acc = await _context.Accounts.FindAsync(Int32.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)));
            Request request = new()
            {
                AccountId = acc.Id,
                CompetitionId= compId,
                Status = "На рассмотрении"
            };
            _context.Add(request);
            await _context.SaveChangesAsync();
            
            return RedirectToAction("Index", "Home");
        }

        [Authorize(Roles = "organizer")]
        [HttpPost]
        public async Task<IActionResult> Update(int id, Request req)
        {
            Request request = await _context.Requests.FindAsync(id);
            request.Status = req.Status;

            if (request.Status == "Принята")
            {
                Account account = await _context.Accounts.FindAsync(request.AccountId);
                account.Competitions.Add(request.Competition);

                _context.Accounts.Update(account);
            }

            _context.Requests.Update(request);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Competition", req.CompetitionId);
        }

        [Authorize(Roles = "sportsman")]
        public async Task<IActionResult> Delete(int id)
        {
            var request = await _context.Requests
                .Include(r => r.Account)
                .Include(r => r.Competition)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (request == null)
            {
                return NotFound();
            }

            return View(request);
        }

        [Authorize(Roles = "sportsman")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var request = await _context.Requests.FindAsync(id);
            _context.Requests.Remove(request);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
