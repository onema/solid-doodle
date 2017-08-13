using System;
using System.Text.RegularExpressions;

namespace LogParser.Model {
    public class User {
        //--- Constants ---
        public const string FILTER = @"\((.*?)\)";
        
        //--- Fields ---
        private static readonly Regex Filter = new Regex(FILTER, RegexOptions.Compiled); 
        
        //--- Properties ---
        public string Name { get; }
        public int Favorite { get; }
        public int TweetCount { get; }
        public int Friends { get; }
        public int Follow { get; }
        
        //--- Constructors ---
        public User(string user) {
            var matches = Filter.Matches(user);
            Name = matches[0].Groups[1].Value;
            Favorite = Convert.ToInt32(matches[1].Groups[1].Value);
            TweetCount = Convert.ToInt32(matches[2].Groups[1].Value);
            Friends = Convert.ToInt32(matches[3].Groups[1].Value);
            Follow = Convert.ToInt32(matches[4].Groups[1].Value);
        }
    }
}
