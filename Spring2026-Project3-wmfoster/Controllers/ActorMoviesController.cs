using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Spring2026_Project3_wmfoster.Data;
using Spring2026_Project3_wmfoster.Models;

namespace Spring2026_Project3_wmfoster.Controllers
{
    public class ActorMoviesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ActorMoviesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.ActorMovies
                .Include(a => a.Actor)
                .Include(a => a.Movie);
            return View(await applicationDbContext.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actorMovie = await _context.ActorMovies
                .Include(a => a.Actor)
                .Include(a => a.Movie)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (actorMovie == null)
            {
                return NotFound();
            }

            return View(actorMovie);
        }

        public IActionResult Create()
        {
            ViewData["ActorId"] = new SelectList(_context.Actors, "Id", "Name");
            ViewData["MovieId"] = new SelectList(_context.Movies, "Id", "Title");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ActorMovie actorMovie)
        {
            bool exists = await _context.ActorMovies
                .AnyAsync(am => am.ActorId == actorMovie.ActorId && am.MovieId == actorMovie.MovieId);

            if (exists)
            {
                ModelState.AddModelError("", "This relationship already exists.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(actorMovie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["ActorId"] = new SelectList(_context.Actors, "Id", "Name", actorMovie.ActorId);
            ViewData["MovieId"] = new SelectList(_context.Movies, "Id", "Title", actorMovie.MovieId);

            return View(actorMovie);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actorMovie = await _context.ActorMovies.FindAsync(id);
            if (actorMovie == null)
            {
                return NotFound();
            }

            ViewData["ActorId"] = new SelectList(_context.Actors, "Id", "Name", actorMovie.ActorId);
            ViewData["MovieId"] = new SelectList(_context.Movies, "Id", "Title", actorMovie.MovieId);

            return View(actorMovie);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ActorId,MovieId")] ActorMovie actorMovie)
        {
            if (id != actorMovie.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(actorMovie);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ActorMovieExists(actorMovie.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["ActorId"] = new SelectList(_context.Actors, "Id", "Name", actorMovie.ActorId);
            ViewData["MovieId"] = new SelectList(_context.Movies, "Id", "Title", actorMovie.MovieId);

            return View(actorMovie);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actorMovie = await _context.ActorMovies
                .Include(a => a.Actor)
                .Include(a => a.Movie)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (actorMovie == null)
            {
                return NotFound();
            }

            return View(actorMovie);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var actorMovie = await _context.ActorMovies.FindAsync(id);
            if (actorMovie != null)
            {
                _context.ActorMovies.Remove(actorMovie);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ActorMovieExists(int id)
        {
            return _context.ActorMovies.Any(e => e.Id == id);
        }
    }
}