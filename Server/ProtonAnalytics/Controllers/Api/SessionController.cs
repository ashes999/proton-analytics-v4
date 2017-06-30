using Newtonsoft.Json.Linq;
using NLog;
using ProtonAnalytics.Models;
using ProtonAnalytics.Repositories;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;

namespace ProtonAnalytics.Controllers.Api
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class SessionController : ApiController
    {
        private static readonly string HaxeDateTimeFormat = "yyyy-MM-dd HH:mm:ss";
        private ILogger logger;

        private IGenericRepository repository;

        public SessionController(IGenericRepository repository, ILogger logger)
        {
            this.repository = repository;
            this.logger = logger;
        }

        // POST: api/Session
        // Creates a new session. Returns true if successful, false if you shouldn't retry, error if you should retry.
        public bool Post([FromBody]JObject json)
        {
            var apiKey = json.GetValue("apiKey").Value<string>();
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                logger.Fatal("Create Session failed: API key is empty");
                return false; // invalid request, don't retry
            }

            var playerId = json.GetValue("playerId").Value<string>();
            if (string.IsNullOrWhiteSpace(playerId))
            {
                logger.Fatal("Create Session failed: player ID is empty");
                return false; // invalid request, don't retry
            }

            var version = json.GetValue("version").Value<string>();
            if (string.IsNullOrWhiteSpace(version))
            {
                logger.Fatal("Create Session failed: version is empty");
                return false; // invalid request, don't retry
            }

            var platform = json.GetValue("platform").Value<string>();
            if (string.IsNullOrWhiteSpace(platform))
            {
                logger.Fatal("Create Session failed: platform is empty");
                return false; // invalid request, don't retry
            }

            var operatingSystem = json.GetValue("operatingSystem").Value<string>();
            if (string.IsNullOrWhiteSpace(operatingSystem))
            {
                logger.Fatal("Create Session failed: OS is empty");
                return false; // invalid request, don't retry
            }

            var sessionStartUtc = json.GetValue("sessionStartUtc").Value<string>();
            if (string.IsNullOrWhiteSpace(sessionStartUtc))
            {
                logger.Fatal("Create Session failed: session start time is empty");
                return false; // invalid request, don't retry
            }
            var asDate = DateTime.ParseExact(sessionStartUtc, HaxeDateTimeFormat, CultureInfo.InvariantCulture);

            var games = repository.Query<Game>("ApiKey = @apiKey", new { apiKey = apiKey });

            if (games.Count() != 1)
            {
                return false; // Your game doesn't exist or you gave us the wrong API key. Don't retry.
            }

            var game = games.Single();

            // Game is legit. Proceed to insert session.
            var session = new Session(game.Id, Guid.Parse(playerId), version, platform, operatingSystem, asDate);
            this.repository.Save<Session>(session);

            return true; 
        }

        // PUT: api/Session/5
        // Marks a session as complete. Returns true if successful, false if you shouldn't retry, error if you should retry.
        public bool Put([FromBody]JObject json)
        {
            var apiKey = json.GetValue("apiKey").Value<string>();
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                logger.Fatal("End Session failed: API key is empty");
                return false; // invalid request, don't retry
            }

            var playerId = json.GetValue("playerId").Value<string>();
            if (string.IsNullOrWhiteSpace(playerId))
            {
                logger.Fatal("End Session failed: player ID is empty");
                return false; // invalid request, don't retry
            }

            var sessionEndUtc = json.GetValue("sessionEndUtc").Value<string>();
            if (string.IsNullOrWhiteSpace(sessionEndUtc))
            {
                logger.Fatal("End Session failed: session end-time is empty");
                return false; // invalid request, don't retry
            }
            var asDate = DateTime.ParseExact(sessionEndUtc, HaxeDateTimeFormat, CultureInfo.InvariantCulture);

            var games = repository.Query<Game>("ApiKey = @apiKey", new { apiKey = apiKey });

            if (games.Count() != 1)
            {
                return false; // Your game doesn't exist or you gave us the wrong API key. Don't retry.
            }

            var game = games.Single();

            // Game is legit. Proceed to insert session.
            var sessions = repository.Query<Session>("GameId = @gameId AND PlayerId = @playerId", new { gameId = game.Id, playerId = playerId });

            if (sessions.Count() == 0)
            {
                return false; // You never started a session!
            }

            // If multiple, end the last session. Even if it has an end time, update it.
            // This allows platforms like Flash, which can't support end-session, to checkpoint
            // every few minutes. Not the best, but better than nothing, amirite?
            var session = sessions.OrderBy(s => s.SessionStartUtc).Last();            
            session.End(asDate);

            repository.Save<Session>(session);

            return true;
        }        
    }
}
