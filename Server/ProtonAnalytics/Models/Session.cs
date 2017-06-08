using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProtonAnalytics.Models
{
    public class Session
    {

        public Guid Id { get; }
        public Guid GameId { get; }
        public Guid PlayerId { get; }
        public DateTime SessionStartUtc { get; }
        public DateTime? SessionEndUtc { get; private set; } // may be null if the client doesn't report this, eg. Flash
        public string Platform { get; }
        public string OperatingSystem { get; }

        // For Dapper.net deserialization
        public Session(Guid id, Guid gameId, System.Guid playerId, DateTime sessionStartUtc, DateTime sessionEndUtc, string platform, string operatingSystem)
        {
            this.Id = id;
            this.GameId = gameId;
            this.PlayerId = playerId;
            this.SessionStartUtc = sessionStartUtc;
            this.SessionEndUtc = sessionEndUtc;
            this.Platform = platform;
            this.OperatingSystem = operatingSystem;
        }

        public Session(Guid gameId, Guid playerId, string platform, string operatingSystem, DateTime sessionStartUtc)
        {
            this.GameId = gameId;
            this.PlayerId = playerId;
            this.Platform = platform;
            this.OperatingSystem = operatingSystem;
            this.SessionStartUtc = DateTime.UtcNow;
        }

        public void End(DateTime endTimeUtc)
        {
            // Possibly overwrites the existing end date. That's okay, because that allows
            // us to support checkpointing on Flash, where there's no reliable end-session.
            this.SessionEndUtc = endTimeUtc;
        }
    }
}