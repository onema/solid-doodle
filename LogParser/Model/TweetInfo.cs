using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace LogParser.Model {
    public class TweetInfo {
        //--- Constants ---
        public const string FILTER = @"\((.*?)\)";
        
        //--- Fields ---
        private static readonly Regex filter = new Regex(FILTER, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Multiline); 
        
        //--- Properties ---
        [JsonProperty(PropertyName="user_name")]
        public string UserName { get; set; }
        
        [JsonProperty(PropertyName="retweeted")]
        public int Retweeted { get; set; }
        
        [JsonProperty(PropertyName="favorited")]
        public int Favorited { get; set; }
        
        [JsonProperty(PropertyName="message")]
        public string Message { get; set; }
        
        [JsonProperty(PropertyName="hashtags")]
        public IEnumerable<string> HashTags { get; set; }
        
        [JsonProperty(PropertyName="latitude")]
        public double Latitude { get; set; }
        
        [JsonProperty(PropertyName="longitude")]
        public double Longitude { get; set; }
        
        [JsonProperty(PropertyName="date_created")]
        public string DateCreated { get; set; }
        
        //--- Constructors ---
        public TweetInfo(string message, string tweetInfo, string hashtags, string location) {
            var messageMatch = filter.Matches(message.Replace("\n", string.Empty));
            UserName = messageMatch[0].Groups[1].Value; 
            Message = messageMatch[1].Groups[1].Value;
            DateCreated = messageMatch[2].Groups[1].Value;
            if(!string.IsNullOrEmpty(tweetInfo)) {
                var tweetInfoMatches = filter.Matches(tweetInfo);
                Retweeted = Convert.ToInt32(tweetInfoMatches[0].Groups[1].Value);
                Favorited = Convert.ToInt32(tweetInfoMatches[1].Groups[1].Value);
            }
            if(!string.IsNullOrEmpty(hashtags)) {
                var hashtagsMatches = filter.Matches(hashtags);
                HashTags = hashtagsMatches[0].Groups[1].Value.Split(',').Select(x => x.Trim());
            }
            if(!string.IsNullOrEmpty(location)) {
                var locationMatches = filter.Matches(location);
                Latitude = Convert.ToDouble(locationMatches[0].Groups[1].Value);
                Longitude = Convert.ToDouble(locationMatches[1].Groups[1].Value);
            }
        }
    }
}
