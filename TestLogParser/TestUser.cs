using System;
using LogParser.Model;
using Xunit;

namespace TestLogParser {
    public class TestUser{
        [Fact]
        public void UserEntrySholdBeParsedCorrectly() {
            // Arrange 
            var user = new User(
                "[USER]: The user name is (Foo) (Foo Bar), they have (8193) favorite tweets and have tweeted (6460) times. They have (661) friends and follow (788) people!! This user was created on (2009-02-13T10:00:41-08:00)");
            
            // Act - Assert
            Assert.Equal("Foo", user.UserName);
            Assert.Equal("Foo Bar", user.Name);
            Assert.Equal(8193, user.Favorite);
            Assert.Equal(6460, user.TweetCount);
            Assert.Equal(661, user.Friends);
            Assert.Equal(788, user.Follow);
        }
    }
}