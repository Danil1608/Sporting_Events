using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sporting_Events.Models;
using Sporting_Events.ViewModels;

namespace Sporting_Events.Controllers
{
    public class CompetitionController : Controller
    {
        private readonly AppDbContext _context;

        public CompetitionController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "organizer")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Competitions.Include(c => c.Accounts).Include(c => c.CompetitionType).Include(c => c.AppFile).ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var competition = await _context.Competitions
                .FirstOrDefaultAsync(m => m.Id == id);
            if (competition == null)
            {
                return NotFound();
            }

            return View(competition);
        }

        [HttpGet]
        [Authorize(Roles = "organizer")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "organizer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Competition dto, IFormFile Upload)
        {
            if (ModelState.IsValid)
            {
                var competition = new Competition
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    Date = dto.Date,
                    ExpDate = dto.ExpDate,
                    Location = dto.Location,
                    MembersCount = dto.MembersCount,
                    PrizePool = dto.PrizePool,
                    CompetitionTypeId = dto.CompetitionTypeId
                };

                if (Upload != null && Upload.Length > 0)
                {
                    using var memoryStream = new MemoryStream();
                    await Upload.CopyToAsync(memoryStream);
                    if (memoryStream.Length < 2097152)
                    {
                        var file = new AppFile
                        {
                            Content = memoryStream.ToArray()
                        };
                        await _context.Files.AddAsync(file);
                        await _context.SaveChangesAsync();
                        competition.AppFileId = file.Id;
                    }
                    else
                    {
                        ModelState.AddModelError("File", "Файл имеет слишком большой размер!");
                    }
                }

                await _context.Competitions.AddAsync(competition);
                await _context.SaveChangesAsync();
                return View(competition);
            }
            return View(dto);
        }

        [HttpGet]
        [Authorize(Roles = "organizer")]
        public async Task<IActionResult> Update(int id)
        {
            var competition = await _context.Competitions.FindAsync(id);
            if (competition == null)
            {
                return NotFound();
            }
            return View(competition);
        }

        [HttpPost]
        [Authorize(Roles = "organizer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, Competition dto, IFormFile Upload)
        {
            var competition = await _context.Competitions.FindAsync(id);

            if (competition == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                competition.Name = dto.Name;
                competition.Description = dto.Description;
                competition.Date = dto.Date;
                competition.ExpDate = dto.ExpDate;
                competition.Location = dto.Location;
                competition.MembersCount = dto.MembersCount;
                competition.PrizePool = dto.PrizePool;
                competition.CompetitionTypeId = dto.CompetitionTypeId;
                if (Upload != null && Upload.Length > 0)
                {
                    using var memoryStream = new MemoryStream();
                    await Upload.CopyToAsync(memoryStream);
                    if (memoryStream.Length < 2097152)
                    {
                        var file = new AppFile
                        {
                            Content = memoryStream.ToArray()
                        };
                        var oldFile = await _context.Files.FindAsync(competition.AppFileId);
                        if (oldFile != null)
                        {
                            _context.Files.Remove(oldFile);
                        }

                        await _context.Files.AddAsync(file);
                        await _context.SaveChangesAsync();
                        competition.AppFileId = file.Id;
                    }
                    else
                    {
                        ModelState.AddModelError("File", "Файл имеет слишком большой размер!");
                    }
                }
                _context.Competitions.Update(competition);
                await _context.SaveChangesAsync();
                return View(competition);
            }
            return View(dto);
        }

        [HttpGet]
        [Authorize(Roles = "organizer")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var competition = await _context.Competitions
                .FirstOrDefaultAsync(m => m.Id == id);
            if (competition == null)
            {
                return NotFound();
            }

            return View(competition);
        }

        [Authorize(Roles = "organizer")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var competition = await _context.Competitions.FindAsync(id);
            _context.Competitions.Remove(competition);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CompetitionExists(int id)
        {
            return _context.Competitions.Any(e => e.Id == id);
        }
    }
}
