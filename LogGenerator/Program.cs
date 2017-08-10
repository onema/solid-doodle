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

    //--- Class ---
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
            StartStreaming(client, sequenceToken);
        }

        private static void StartStreaming(IAmazonCloudWatchLogs client, string sequenceToken) {
            var counter = 0;
            var logEventsBatch = new List<InputLogEvent>();
            var stream = Stream.CreateFilteredStream();
            stream.AddTrack("cats");
            // stream.AddLocation(new Coordinates(-74, 40), new Coordinates(-73, 41));
            stream.MatchingTweetReceived += (sender, eventArgs) => {
                Console.WriteLine(eventArgs.Tweet);
                
                var tweetInfo = JsonConvert.DeserializeObject<JObject>(eventArgs.Tweet.ToJson());
                var user = tweetInfo["user"];
                var retweetedStatus = tweetInfo["retweeted_status"];
                var hashTags = String.Join(", ", retweetedStatus.Select(x => $"${x["hashtags"].ToArray()}"));
                var text = $"The user name is '{user["name"]}', they have {user["favourites_count"]} favorite tweets and have tweeted {user["statuses_count"]} times. They have {user["friends_count"]} friends and follow {user["followers_count"]} people!!\u03BB#"; 
                text += $"This tweet has been retweeted {retweetedStatus["retweet_count"]} and have been favorited by {retweetedStatus["favorite_count"]}\u03BB#";
                text += $"This twee has the follwoing hash tags: [{hashTags}]\u03BB#";
                
                logEventsBatch.Add(new InputLogEvent {
                    Message = text,
                    Timestamp = DateTime.Now
                });
                if(++counter % 10 == 0) {
                    sequenceToken = DispatchLogEvents(client, logEventsBatch, sequenceToken);
                    logEventsBatch = new List<InputLogEvent>();
                }
            };
            stream.StartStreamMatchingAllConditions();
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
