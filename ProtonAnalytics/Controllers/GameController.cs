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
            return View(games);
        }

        // GET: Game/Details/5
        public ActionResult Details(int id)
        {
            var game = this.repository.Query<Game>("Id = @id AND OwnerId = @currentUserId", new { id = id, currentUserId = this.CurrentUserId });
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
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Game/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
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
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Game/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
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
    }
}
