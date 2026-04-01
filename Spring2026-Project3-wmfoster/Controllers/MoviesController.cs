using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spring2026_Project3_wmfoster.Data;
using Spring2026_Project3_wmfoster.Models;
using Spring2026_Project3_wmfoster.Services;
using Spring2026_Project3_wmfoster.ViewModels;

namespace Spring2026_Project3_wmfoster.Controllers
{
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AiService _aiService;
        private readonly SentimentService _sentimentService;

        public MoviesController(ApplicationDbContext context, AiService aiService, SentimentService sentimentService)
        {
            _context = context;
            _aiService = aiService;
            _sentimentService = sentimentService;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Movies.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies
                .Include(m => m.ActorMovies)
                .ThenInclude(am => am.Actor)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null)
            {
                return NotFound();
            }

            var reviews = await _aiService.GenerateMovieReviewsAsync(movie.Title, movie.Genre, movie.YearOfRelease);

            var reviewItems = reviews.Select(r => new MovieReviewItemViewModel
            {
                Review = r,
                Sentiment = _sentimentService.GetSentimentLabel(r)
            }).ToList();

            var vm = new MovieDetailsViewModel
            {
                Movie = movie,
                Actors = movie.ActorMovies.Select(am => am.Actor!.Name).ToList(),
                Reviews = reviewItems,
                AverageSentiment = _sentimentService.GetAverageSentiment(reviews)
            };

            return View(vm);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Movie movie, IFormFile? posterFile)
        {
            if (posterFile != null && posterFile.Length > 0)
            {
                using var ms = new MemoryStream();
                await posterFile.CopyToAsync(ms);
                movie.Poster = ms.ToArray();
            }

            if (ModelState.IsValid)
            {
                _context.Add(movie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(movie);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Movie movie, IFormFile? posterFile)
        {
            if (id != movie.Id)
            {
                return NotFound();
            }

            var existingMovie = await _context.Movies.FindAsync(id);
            if (existingMovie == null)
            {
                return NotFound();
            }

            existingMovie.Title = movie.Title;
            existingMovie.ImdbLink = movie.ImdbLink;
            existingMovie.Genre = movie.Genre;
            existingMovie.YearOfRelease = movie.YearOfRelease;

            if (posterFile != null && posterFile.Length > 0)
            {
                using var ms = new MemoryStream();
                await posterFile.CopyToAsync(ms);
                existingMovie.Poster = ms.ToArray();
            }

            if (ModelState.IsValid)
            {
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(movie);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie != null)
            {
                _context.Movies.Remove(movie);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(int id)
        {
            return _context.Movies.Any(e => e.Id == id);
        }
    }
}