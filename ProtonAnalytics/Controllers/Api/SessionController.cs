using ProtonAnalytics.Models;
using ProtonAnalytics.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ProtonAnalytics.Controllers.Api
{
    public class SessionController : ApiController
    {
        private IGenericRepository repository;

        public SessionController(IGenericRepository repository)
        {
            this.repository = repository;
        }

        // POST: api/Session
        // Creates a new session. Returns true if successful, false if you shouldn't retry, error if you should retry.
        public bool Post([FromBody]string apiKey, [FromBody]Guid playerId, [FromBody]string platform)
        {
            var games = repository.Query<Game>("ApiKey = @apiKey", new { apiKey = apiKey });

            if (games.Count() != 1)
            {
                return false; // Your game doesn't exist or you gave us the wrong API key. Don't hammer us with sessions.
            }

            var game = games.Single();

            // Game is legit. Proceed to insert session.
            var session = new Session(game.Id, playerId, platform);
            this.repository.Save<Session>(session);

            return true; 
        }

        // PUT: api/Session/5
        // Marks a session as complete. Returns true if successful, false if you shouldn't retry, error if you should retry.
        public bool Put([FromBody]string apiKey, [FromBody]Guid playerId)
        {
            // Look up the session. Player has only one active session at a time.
            // If there are multiple (eg. Flash, player starts session, end-session never triggered),
            // take the last one and use that as the active session.

            // If there's no session, return false.

            // Update SessionEndUtc
            return true;
        }        
    }
}
