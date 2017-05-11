using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace ProtonAnalytics.App_Start
{

    public class FeatureConfig
    {
        private readonly string configFileName;
        private IDictionary<string, dynamic> contents;
        private DateTime lastWriteTimeUtc = DateTime.MinValue;
        private readonly Regex CommentsRegex = new Regex("//.*");

        public static FeatureConfig LastInstance { get; private set; }

        public FeatureConfig(string configFileName)
        {
            this.configFileName = configFileName;
            this.contents = new Dictionary<string, dynamic>();
            this.UpdateContentsIfFileChanged();
            FeatureConfig.LastInstance = this;
        }

        public T Get<T>(string key) where T : struct
        {
            this.UpdateContentsIfFileChanged();
            return (T)this.contents[key];
        }

        private void UpdateContentsIfFileChanged()
        {
            var currentWriteTime = File.GetLastWriteTimeUtc(this.configFileName);
            if (lastWriteTimeUtc != currentWriteTime)
            {
                lastWriteTimeUtc = currentWriteTime;

                var configContents = File.ReadAllText(this.configFileName);

                // Remove comments
                configContents = CommentsRegex.Replace(configContents, "");
                dynamic json = JsonConvert.DeserializeObject(configContents);
                foreach (var kvp in json)
                {
                    JToken token = kvp.Value as JToken;
                    if (token.Type == JTokenType.Boolean)
                    {
                        this.contents[kvp.Name.ToString()] = bool.Parse(kvp.Value.ToString());
                    }
                    else if (token.Type == JTokenType.Integer)
                    {
                        this.contents[kvp.Name.ToString()] = int.Parse(kvp.Value.ToString());
                    }
                    else
                    {
                        // Dump it as-is. Get<T> will likely fail because it's a JSomethingType.
                        this.contents[kvp.Name] = kvp.Value;
                    }
                }
            }            
        }
    }
}