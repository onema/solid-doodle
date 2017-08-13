using System;
using System.Collections.Generic;
using System.Linq;
using LogParser;
using LogParser.Model;
using Xunit;

namespace TestLogParser {
    public class TestTweetInfo {
        [Fact]
        public void TweeterInfoEntrySholdBeParsedCorrectly() {
            // Arrange 
            var info = new TweetInfo(
                "[MESSAGE]: The tweet message is: (RT @DaytonMSoccer: COMING SOON. Dayton Men's Soccer on Instagram. Launching Monday. Be sure to check us out.)",
                "[TWEET_INFO]: This tweet has been retweeted (2) times, and have been favorited by (4) people",
                "[HASH_TAGS]: This tweet has the following hash tags: (FlyTogether)",
                "[LOCATION]: The location of this tweet is lat: (123.12), long: (-123.12)"
            );
            
            // Act - Assert
            Assert.Equal("RT @DaytonMSoccer: COMING SOON. Dayton Men's Soccer on Instagram. Launching Monday. Be sure to check us out.", info.Message);
            Assert.Equal(2, info.Retweeted);
            Assert.Equal(4, info.Favorited);
            Assert.Equal("FlyTogether", info.HashTags.First());
            Assert.Equal(123.12, info.Latitude);
            Assert.Equal(-123.12, info.Longitude);
        }

        [Fact]
        public void GetJsonValueShouldBeParsedCorrectly() {
            var tweetInfo = new List<List<string>> {
                new List<string> {
                    "[MESSAGE]: The tweet message is: (RT @DaytonMSoccer: COMING SOON. Dayton Men's Soccer on Instagram. Launching Monday. Be sure to check us out.)",
                    "[TWEET_INFO]: This tweet has been retweeted (2) times, and have been favorited by (4) people",
                    "[HASH_TAGS]: This tweet has the following hash tags: (FlyTogether)",
                    "[LOCATION]: The location of this tweet is lat: (123.12), long: (-123.12)"
                }
            };
            Function.TweetJson(tweetInfo);
        }

        [Fact]
        public void GetJsonValueWithMessageWithLineBreaksShouldBeParsedCorrectly() {
            var tweetInfo = new List<List<string>> {
                new List<string> {
                    "[MESSAGE]: The tweet message is: (#insiders #auspol\n" +
                    "Gays are barely 1% of our population.\n" +
                    "Dont believe @InsidersABC hype #SSM.\n" +
                    "Put children 1st\n" +
                    "Lo… https://t.co/9dYuJ1OmuJ)\n",
                    "[TWEET_INFO]: This tweet has been retweeted (2) times, and have been favorited by (4) people",
                    "[HASH_TAGS]: This tweet has the following hash tags: (FlyTogether)",
                    "[LOCATION]: The location of this tweet is lat: (123.12), long: (-123.12)"
                }
            };
            var json = Function.TweetJson(tweetInfo);
            Assert.IsType<string>(json);
        }
    }
}