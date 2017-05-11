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

        public Session(Guid gameId, Guid playerId, string platform)
        {
            this.GameId = gameId;
            this.PlayerId = playerId;
            this.Platform = platform;
            this.SessionStartUtc = DateTime.UtcNow;
        }

        public void End()
        {
            this.SessionEndUtc = DateTime.UtcNow;
        }
    }
}