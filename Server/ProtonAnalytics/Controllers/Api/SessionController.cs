using Newtonsoft.Json.Linq;
using NLog;
using ProtonAnalytics.App_Start.RuntimeConfiguration;
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
            bool useGetAndPostOnly = FeatureConfig.LastInstance.Get<bool>("ApiUsesGetAndPostVerbsOnly");
            bool proceed = false;

            if (useGetAndPostOnly)
            {
                // Route to the PUT call. Detect based on a unique field (sessionEndUtc).
                JToken token = this.TryGetValue(json, "sessionEndUtc", "EndSession");
                if (token != null)
                {
                    // Probably a valid call.
                    return this.Put(json);
                }
                else
                {
                    // Routing is enabled but this is just a regular POST / StartSession call
                    proceed = true;
                }
            }
            else
            {
                // regular POST call
                proceed = true;
            }
            
            if (proceed)
            {
                // Creates a new session

                var token = this.TryGetValue(json, "apiKey", "StartSession");
                if (token == null)
                {
                    return false; // invalid request, don't retry
                }
                var apiKey = token.Value<string>();

                token = this.TryGetValue(json, "playerId", "StartSession");
                if (token == null)
                {
                    return false; // invalid request, don't retry
                }
                var playerId = token.Value<string>();

                token = this.TryGetValue(json, "version", "StartSession");
                if (token == null)
                {
                    return false; // invalid request, don't retry
                }
                var version = token.Value<string>();

                token = this.TryGetValue(json, "platform", "StartSession");
                if (token == null)
                {
                    return false; // invalid request, don't retry
                }
                var platform = token.Value<string>();

                token = this.TryGetValue(json, "operatingSystem", "StartSession");
                if (token == null)
                {
                    return false; // invalid request, don't retry
                }
                var operatingSystem = token.Value<string>();

                token = this.TryGetValue(json, "sessionStartUtc", "StartSession");
                if (token == null)
                {
                    return false; // invalid request, don't retry
                }
                var sessionStartUtc = token.Value<string>();

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

            // Technically not possible but required to compile
            return false;
        }

        // PUT: api/Session/5
        // Marks a session as complete. Returns true if successful, false if you shouldn't retry, error if you should retry.
        public bool Put([FromBody]JObject json)
        {
            var token = this.TryGetValue(json, "apiKey", "EndSession");
            if (token == null)
            {
                return false; // invalid request, don't retry
            }
            var apiKey = token.Value<string>();

            token = this.TryGetValue(json, "playerId", "EndSession");
            if (token == null)
            {
                return false; // invalid request, don't retry
            }
            var playerId = token.Value<string>();

            token = this.TryGetValue(json, "sessionEndUtc", "EndSession");
            if (token == null)
            {
                return false; // invalid request, don't retry
            }
            var sessionEndUtc = token.Value<string>();

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

        /// <summary>
        /// Get a property from a JSON object. If the property doesn't exist, logs an error.
        /// If the property value is empty, also logs an error.
        /// Returns null if the property doesn't exist or is empty; returns the JToken node otherwise.
        /// </summary>
        private JToken TryGetValue(JObject obj, string propertyName, string errorMessagePrefix)
        {
            JToken toReturn = null;
            var succeeded = obj.TryGetValue(propertyName, out toReturn);
            if (!succeeded)
            {
                logger.Error($"{errorMessagePrefix}: {propertyName} doesn't exist");
                return null;
            }
            else if (string.IsNullOrWhiteSpace(toReturn.Value<string>()))
            {
                logger.Error($"{errorMessagePrefix}: {propertyName} is empty");
                return null;
            }
            return toReturn;
        }
    }
}
