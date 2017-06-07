using ProtonAnalytics.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProtonAnalytics.Repositories;

namespace ProtonAnalytics.Controllers
{
    public class GameController : AbstractController
    {
        public GameController(IGenericRepository repository) : base(repository)
        {
        }

        // GET: Game
        public ActionResult Index()
        {
            var games = this.repository.Query<Game>("OwnerId = @currentUserId", new { currentUserId = this.CurrentUserId });
            ViewBag.GameStats = GenerateStatsFor(games);
            return View(games);
        }

        // GET: Game/Details/5
        public ActionResult Details(Guid id)
        {
            var game = this.repository.Query<Game>("Id = @id AND OwnerId = @currentUserId", new { id = id, currentUserId = this.CurrentUserId }).Single();
            ViewBag.GameStats = GenerateStatsFor(game);
            return View(game);
        }
        
        // GET: Game/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Game/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here
                var game = new Game();
                game.Name = collection["Name"];
                game.OwnerId = this.CurrentUserId.ToString();
                game.GenerateApiKey();
                this.repository.Save<Game>(game);
                this.Flash($"Game {game.Name} created.");
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Game/Edit/5
        public ActionResult Edit(Guid id)
        {
            return View();
        }

        // POST: Game/Edit/5
        [HttpPost]
        public ActionResult Edit(Guid id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Game/Delete/5
        public ActionResult Delete(Guid id)
        {
            return View();
        }

        // POST: Game/Delete/5
        [HttpPost]
        public ActionResult Delete(Guid id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        private Dictionary<Guid, GameStats> GenerateStatsFor(Game game)
        {
            return this.GenerateStatsFor(new List<Game>() { game });
        }

        private Dictionary<Guid, GameStats> GenerateStatsFor(IEnumerable<Game> games)
        {
            var toReturn = new Dictionary<Guid, GameStats>();

            foreach (var game in games)
            {
                var stats = new GameStats();
                var sessions = this.repository.Query<Session>("GameId = @gameId", new { gameId = game.Id });

                stats.NumSessions = sessions.Count();
                var finishedSessions = sessions.Where(s => s.SessionEndUtc != DateTime.MinValue); // nullable but comes back as DateTime.MinValue
                var totalSeconds = finishedSessions.Average(s => (s.SessionEndUtc.Value - s.SessionStartUtc).TotalSeconds);
                stats.AverageSessionTimeSeconds = totalSeconds;
                toReturn[game.Id] = stats;
            }

            return toReturn;
        }
    }

    public class GameStats
    {
        public int NumSessions { get; set; }
        public double AverageSessionTimeSeconds { get; set; }
    }
}
