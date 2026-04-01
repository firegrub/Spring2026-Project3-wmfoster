using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spring2026_Project3_wmfoster.Data;
using Spring2026_Project3_wmfoster.Models;
using Spring2026_Project3_wmfoster.Services;
using Spring2026_Project3_wmfoster.ViewModels;

namespace Spring2026_Project3_wmfoster.Controllers
{
    public class ActorsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AiService _aiService;
        private readonly SentimentService _sentimentService;

        public ActorsController(ApplicationDbContext context, AiService aiService, SentimentService sentimentService)
        {
            _context = context;
            _aiService = aiService;
            _sentimentService = sentimentService;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Actors.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actors
                .Include(a => a.ActorMovies)
                .ThenInclude(am => am.Movie)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (actor == null)
            {
                return NotFound();
            }

            var tweets = await _aiService.GenerateActorTweetsAsync(actor.Name);

            var tweetItems = tweets.Select(t => new ActorTweetItemViewModel
            {
                Tweet = t,
                Sentiment = _sentimentService.GetSentimentLabel(t)
            }).ToList();

            var vm = new ActorDetailsViewModel
            {
                Actor = actor,
                Movies = actor.ActorMovies.Select(am => am.Movie!.Title).ToList(),
                Tweets = tweetItems,
                OverallSentiment = _sentimentService.GetAverageSentiment(tweets)
            };

            return View(vm);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Actor actor, IFormFile? photoFile)
        {
            if (photoFile != null && photoFile.Length > 0)
            {
                using var ms = new MemoryStream();
                await photoFile.CopyToAsync(ms);
                actor.Photo = ms.ToArray();
            }

            if (ModelState.IsValid)
            {
                _context.Add(actor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(actor);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actors.FindAsync(id);
            if (actor == null)
            {
                return NotFound();
            }

            return View(actor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Actor actor, IFormFile? photoFile)
        {
            if (id != actor.Id)
            {
                return NotFound();
            }

            var existingActor = await _context.Actors.FindAsync(id);
            if (existingActor == null)
            {
                return NotFound();
            }

            existingActor.Name = actor.Name;
            existingActor.Gender = actor.Gender;
            existingActor.Age = actor.Age;
            existingActor.ImdbLink = actor.ImdbLink;

            if (photoFile != null && photoFile.Length > 0)
            {
                using var ms = new MemoryStream();
                await photoFile.CopyToAsync(ms);
                existingActor.Photo = ms.ToArray();
            }

            if (ModelState.IsValid)
            {
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(actor);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actors
                .FirstOrDefaultAsync(m => m.Id == id);

            if (actor == null)
            {
                return NotFound();
            }

            return View(actor);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var actor = await _context.Actors.FindAsync(id);
            if (actor != null)
            {
                _context.Actors.Remove(actor);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ActorExists(int id)
        {
            return _context.Actors.Any(e => e.Id == id);
        }
    }
}