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
        // may be temporarily null if the client doesn't report this, eg. Flash/HTML5. The client will periodically send
        // an endSession event, eg. every minute, so we should have some sort of value for web clients.
        public DateTime? SessionEndUtc { get; private set; }
        public string Platform { get; }
        public string OperatingSystem { get; }
        public string Version { get; }

        // For Dapper.net deserialization
        public Session(Guid id, Guid gameId, System.Guid playerId, string version, DateTime sessionStartUtc, DateTime sessionEndUtc, string platform, string operatingSystem)
        {
            this.Id = id;
            this.GameId = gameId;
            this.PlayerId = playerId;
            this.SessionStartUtc = sessionStartUtc;
            this.SessionEndUtc = sessionEndUtc;
            this.Platform = platform;
            this.OperatingSystem = operatingSystem;
            this.Version = version;
        }

        public Session(Guid gameId, Guid playerId, string version, string platform, string operatingSystem, DateTime sessionStartUtc)
        {
            this.GameId = gameId;
            this.PlayerId = playerId;
            this.Version = version;
            this.Platform = platform;
            this.OperatingSystem = operatingSystem;
            this.SessionStartUtc = DateTime.UtcNow;
        }

        public void End(DateTime endTimeUtc)
        {
            // Possibly overwrites the existing end date. That's okay, because that allows
            // us to support checkpointing on Flash/HTML5, where there's no reliable end-session.
            this.SessionEndUtc = endTimeUtc;
        }
    }
}