using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProtonAnalytics.Models
{
    public class Game
    {
        private const int ApiSecretSizeInBytes = 32;

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ApiKey { get; internal set; }
        public string OwnerId { get; internal set; }
        public DateTime CreatedOnUtc { get; }

        public Game()
        {
            this.CreatedOnUtc = DateTime.UtcNow;            
        }

        public void GenerateApiKey()
        {
            if (string.IsNullOrWhiteSpace(this.ApiKey))
            {
                // http://stackoverflow.com/questions/14412132/best-approach-for-generating-api-key#18730859
                var key = new byte[ApiSecretSizeInBytes];
                using (var generator = System.Security.Cryptography.RandomNumberGenerator.Create())
                {
                    generator.GetBytes(key);
                }
                this.ApiKey = Convert.ToBase64String(key);
            }
        }
    }
}