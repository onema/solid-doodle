using System;
using System.Collections.Generic;
using Amazon;
using Amazon.CloudWatchLogs;
using Amazon.CloudWatchLogs.Model;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Tweetinvi;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LogGenerator {

    //--- Classes ---
    public static class JsonExtensions {
        public static bool IsNullOrEmpty(this JToken token) {
            return (token == null) ||
                   (token.Type == JTokenType.Array && !token.HasValues) ||
                   (token.Type == JTokenType.Object && !token.HasValues) ||
                   (token.Type == JTokenType.String && token.ToString() == String.Empty) ||
                   (token.Type == JTokenType.Null);
        }
    }
    
    class Program {

        //--- Constants ---
        const string AWS_REGION = "us-west-2";
        const string LOG_GROUP = "/lambda-sharp/log-parser/dev";
        const string LOG_STREAM = "test-log-stream";

        //--- Methods ---
        static void Main(string[] args) {
            var region = RegionEndpoint.GetBySystemName(AWS_REGION);
            var chain = new CredentialProfileStoreChain();
            AWSCredentials awsCredentials;
            if(!chain.TryGetAWSCredentials("lambdasharp", out awsCredentials)) {
                throw new Exception("AWS Credentials not found!");
            }
            var client = new AmazonCloudWatchLogsClient(awsCredentials, region);
            var sequenceToken = GetNextSequenceToken(client);
            SetTwitterCredentials();
            try {
                StartStreaming(client, sequenceToken);
            } catch(Exception e) {
                Console.WriteLine(e);
            }
        }

        private static void StartStreaming(IAmazonCloudWatchLogs client, string sequenceToken) {
            var counter = 0;
            var logEventsBatch = new List<InputLogEvent>();
            var stream = Stream.CreateFilteredStream();
            stream.AddTrack("cats");
            // stream.AddLocation(new Coordinates(-74, 40), new Coordinates(-73, 41));
            stream.MatchingTweetReceived += (sender, eventArgs) => {
                var json = eventArgs.Tweet.ToJson();
                var tweetInfo = JsonConvert.DeserializeObject<JObject>(json);
                logEventsBatch.Add(new InputLogEvent {
                    Message = GetLogText(tweetInfo),
                    Timestamp = DateTime.Now
                });
                if(++counter % 10 == 0) {
                    sequenceToken = DispatchLogEvents(client, logEventsBatch, sequenceToken);
                    logEventsBatch = new List<InputLogEvent>();
                }
            };
            stream.StartStreamMatchingAllConditions();
        }

        private static string GetLogText(JObject tweetInfo) {
            var user = tweetInfo["user"];
            var retweetedStatus = tweetInfo["retweeted_status"];
            var entities = tweetInfo["entities"];
            var hashTags = entities["hashtags"];
            var text = $"The user name is '{user["name"]}', they have {user["favourites_count"]} favorite tweets and have tweeted {user["statuses_count"]} times. They have {user["friends_count"]} friends and follow {user["followers_count"]} people!!\u03BB#";
            if (!retweetedStatus.IsNullOrEmpty()) {
                var retweetCount = retweetedStatus["retweet_count"];
                var favoriteCount = retweetedStatus["favorite_count"];
                text += $"This tweet has been retweeted {retweetCount} times, and have been favorited by {favoriteCount} people\u03BB#";
            }
            if (!hashTags.IsNullOrEmpty()) {
                var hashTagsString = String.Join(", ", hashTags.Select(x => $"{x["text"]}"));
                text += $"This tweet has the following hash tags: [{hashTagsString}]\u03BB#";
            }
            text += $"The tweet message is: [{tweetInfo["text"]}]\u03BB#";
            return text;
        }

        private static void SetTwitterCredentials() {

            // Set up twitter credentials (https://apps.twitter.com)
            var consumerKey = "kOr9jFO0N0oFWEilI74ZTmaop";
            var consumerSecret = "eI1Tjv0tgO9nkYxAJfMslVJRRwJqKsosFwEjEbewYOZDRZhGll";
            var accessToken = "820470222-yHssA3Qt7qNDX5A47VrR0UQJXuWARlcTL6Gok9Ut";
            var accessSecret = "DK1d9WzGKBEAqH0HzFTChDdgsxUpjMhcUzDA2nmy6j0xX";
            Auth.SetUserCredentials(consumerKey, consumerSecret, accessToken, accessSecret);
        }
        
        private static string GetNextSequenceToken(IAmazonCloudWatchLogs client) {
            var response = client.DescribeLogStreamsAsync(new DescribeLogStreamsRequest {
                LogGroupName = LOG_GROUP
            }).Result;
            return response.LogStreams.First(x => x.LogStreamName == LOG_STREAM).UploadSequenceToken;
        }

        public static string DispatchLogEvents(IAmazonCloudWatchLogs client, List<InputLogEvent> logEvents, string sequenceToken) {
            Console.WriteLine("Dispatching Log Events.");
            var request = new PutLogEventsRequest {
                  LogGroupName = LOG_GROUP,
                  LogStreamName = LOG_STREAM,
                  LogEvents = logEvents,
                  SequenceToken = sequenceToken
            };
            var response = client.PutLogEventsAsync(request).Result;
            return response.NextSequenceToken;
        }
    }
}
