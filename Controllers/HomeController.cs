using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sporting_Events.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Sporting_Events.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        public HomeController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(int option = 0)
        {
            if (option == 1)
            {
                return View(await _context.Competitions.Include(x => x.AppFile).Include(c => c.Accounts)
                    .Include(c => c.CompetitionType).Where(x => DateTime.Now.CompareTo(x.Date) < 0).ToListAsync());
            }

            if (option == 2)
            {
                return View(await _context.Competitions.Include(x => x.AppFile).Include(c => c.Accounts)
                    .Include(c => c.CompetitionType).Where(x => DateTime.Now.CompareTo(x.ExpDate) > 0).ToListAsync());
            }

            return View(await _context.Competitions.Include(x => x.AppFile).Include(c => c.Accounts).Include(c => c.CompetitionType).ToListAsync());
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
