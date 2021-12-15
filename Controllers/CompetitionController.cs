using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
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
            var competitions = await _context.Competitions
                .Include(x => x.AppFile)
                .Include(x => x.Accounts)
                .Where(x => x.OrganizerId == Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier)))
                .ToListAsync();
            return View(competitions);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var competition = await _context.Competitions
                .Include(x => x.AppFile)
                .Include(x => x.Accounts)
                .ThenInclude(x => x.Results)
                .Include(x => x.Requests)
                .ThenInclude(x => x.Account)
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
        public async Task<IActionResult> Create(Competition dto, IFormFile upload)
        {
            if (ModelState.IsValid)
            {
                if (dto.Date.CompareTo(dto.ExpDate) > 0)
                {
                    ModelState.AddModelError("Date", "Указана неправильная дата!");
                    return View(dto);
                }

                var competition = new Competition
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    Date = dto.Date,
                    ExpDate = dto.ExpDate,
                    Location = dto.Location,
                    MembersCount = dto.MembersCount,
                    PrizePool = dto.PrizePool,
                    CompetitionTypeId = dto.CompetitionTypeId,
                    OrganizerId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier))
                };

                if (upload != null)
                {
                    if (upload.Length > 0 && (upload.ContentType == "image/jpeg" || upload.ContentType == "image/png"))
                    {
                        using var memoryStream = new MemoryStream();
                        await upload.CopyToAsync(memoryStream);
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
                            ModelState.AddModelError("AppFile", "Файл имеет слишком большой размер!");
                            return View(dto);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("AppFile", "Выбран некорректный файл!");
                        return View(dto);
                    }
                }

                await _context.Competitions.AddAsync(competition);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Competition");
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
        public async Task<IActionResult> Update(int id, Competition dto, IFormFile upload)
        {
            var competition = await _context.Competitions.FindAsync(id);

            if (competition == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (dto.Date.CompareTo(dto.ExpDate) > 0)
                {
                    ModelState.AddModelError("Date", "Указана неправильная дата!");
                    return View(dto);
                }

                competition.Name = dto.Name;
                competition.Description = dto.Description;
                competition.Date = dto.Date;
                competition.ExpDate = dto.ExpDate;
                competition.Location = dto.Location;
                competition.MembersCount = dto.MembersCount;
                competition.PrizePool = dto.PrizePool;
                competition.CompetitionTypeId = dto.CompetitionTypeId;

                if (upload != null)
                {
                    if (upload.Length > 0 && (upload.ContentType == "image/jpeg" || upload.ContentType == "image/png"))
                    {
                        using var memoryStream = new MemoryStream();
                        await upload.CopyToAsync(memoryStream);
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
                            ModelState.AddModelError("AppFile", "Файл имеет слишком большой размер!");
                            return View(dto);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("AppFile", "Выбран некорректный файл!");
                        return View(dto);
                    }
                }
                _context.Competitions.Update(competition);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Competition");
            }
            return View(dto);
        }

        [HttpGet]
        [Authorize(Roles = "organizer")]
        public async Task<IActionResult> Delete(int id)
        {
            var competition = await _context.Competitions.Include(c => c.AppFile).FirstOrDefaultAsync(c => c.Id == id);

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

            if (competition.OrganizerId != Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier)))
            {
                return Forbid();
            }

            _context.Competitions.Remove(competition);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
