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
                .Include(x => x.CompetitionType)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (competition == null)
            {
                return NotFound();
            }

            var results = await _context.Results.Include(x => x.Account).Where(x => x.CompetitionId == id).ToListAsync();

            if (competition.CompetitionType.Name == "running" || competition.CompetitionType.Name == "longjumping")
            {
                results.Sort((a, b) =>
                {
                    if (a == null)
                    {
                        if (b == null)
                        {
                            return 0;
                        }

                        return -1;
                    }
                    else
                    {
                        if (b == null)
                        {
                            return 1;
                        }
                        else
                        {
                            var first = Convert.ToDouble(a.CompResult);
                            var second = Convert.ToDouble(b.CompResult);

                            if (Math.Abs(first - second) < 1e-6)
                            {
                                return 0;
                            }
                            else
                            {
                                return first > second ? 1 : -1;
                            }
                        }
                    }
                });
            }

            if (competition.CompetitionType.Name == "longjumping")
            {
                results.Reverse();
            }

            if (competition.CompetitionType.Name == "rod")
            {
                results.Sort((a, b) =>
                {
                    if (a == null)
                    {
                        if (b == null)
                        {
                            return 0;
                        }

                        return 1;
                    }
                    else
                    {
                        if (b == null)
                        {
                            return -1;
                        }
                        else
                        {
                            var first = Convert.ToInt32(a.CompResult);
                            var second = Convert.ToInt32(b.CompResult);

                            if (first == second)
                            {
                                return 0;
                            }
                            else
                            {
                                return first > second ? -1 : 1;
                            }
                        }
                    }
                });
            }
            for (int i = 1; i <= results.Count; ++i)
            {
                if (results[i].AccountId == results[i - 1].AccountId)
                {
                    results.RemoveAt(i);
                    --i;
                }
            }

            ViewBag.Results = results;

            return View(competition);
        }

        [HttpPost]
        [Authorize(Roles = "organizer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(int competitionId, string res, int accountId)
        {
            var competition = await _context.Competitions
                .Include(x => x.CompetitionType)
                .FirstOrDefaultAsync(x => x.Id == competitionId);

            var account = await _context.Accounts
                .Include(x => x.Competitions)
                .FirstOrDefaultAsync(x => x.Id == accountId);

            if (competition == null)
            {
                return NotFound(new { error = "Соревнование не найдено!" });
            }

            if (account == null)
            {
                return NotFound(new { error = "Участник соревнования не найден!" });
            }

            if (account.Competitions.All(x => x.Id != competitionId))
            {
                return BadRequest(new { error = "Спортсмен не зарегестрирован в данном соревновании!" });
            }

            var result = new Result
            {
                AccountId = accountId,
                CompetitionId = competitionId,
                CompResult = ""
            };

            if (competition.CompetitionType.Name == "rod")
            {
                try
                {
                    var tmp = Convert.ToInt32(res);
                }
                catch (FormatException exception)
                {
                    return BadRequest(new { error = "Результат записан некорректно!" });
                }

                result.CompResult = res;
            }

            if (competition.CompetitionType.Name == "running" || competition.CompetitionType.Name == "longjumping")
            {
                try
                {
                    var tmp = Convert.ToDouble(res);
                }
                catch (FormatException exception)
                {
                    return BadRequest(new { error = "Результат записан некорректно!" });
                }

                result.CompResult = res;
            }

            await _context.Results.AddAsync(result);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Competition", new { id = competitionId });
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
