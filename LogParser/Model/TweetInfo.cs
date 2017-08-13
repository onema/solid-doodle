using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace LogParser.Model {
    public class TweetInfo {
        //--- Constants ---
        public const string FILTER = @"\((.*?)\)";
        
        //--- Fields ---
        private static readonly Regex filter = new Regex(FILTER, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Multiline); 
        
        //--- Properties ---
        public int Retweeted { get; }
        public int Favorited { get; }
        public string Message { get; }
        public IEnumerable<string> HashTags { get; }
        public double Latitude { get; }
        public double Longitude { get; }
        
        //--- Constructors ---
        public TweetInfo(string message, string tweetInfo, string hashtags, string location) {
            var messageMatch = filter.Matches(message.Replace("\n", string.Empty));
            Message = messageMatch[0].Groups[1].Value;
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
