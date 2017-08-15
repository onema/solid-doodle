using System;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace LogParser.Model {
    public class User {
        //--- Constants ---
        public const string FILTER = @"\((.*?)\)";
        
        //--- Fields ---
        private static readonly Regex Filter = new Regex(FILTER, RegexOptions.Compiled); 
        
        //--- Properties ---
        [JsonProperty(PropertyName="name")]
        public string Name { get; set; }
        
        [JsonProperty(PropertyName="favorite")]
        public int Favorite { get; set; }
        
        [JsonProperty(PropertyName="tweet_count")]
        public int TweetCount { get; set; }
        
        [JsonProperty(PropertyName="friends")]
        public int Friends { get; set; }
        
        [JsonProperty(PropertyName="follow")]
        public int Follow { get; set; }
        
        [JsonProperty(PropertyName="date_created")]
        public string DateCreated { get; set; }
        
        //--- Constructors ---
        public User(string user) {
            var matches = Filter.Matches(user);
            Name = matches[0].Groups[1].Value;
            Favorite = Convert.ToInt32(matches[1].Groups[1].Value);
            TweetCount = Convert.ToInt32(matches[2].Groups[1].Value);
            Friends = Convert.ToInt32(matches[3].Groups[1].Value);
            Follow = Convert.ToInt32(matches[4].Groups[1].Value);
            DateCreated = matches[5].Groups[1].Value;
        }
    }
}
