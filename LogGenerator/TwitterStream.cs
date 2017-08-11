using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Amazon;
using Amazon.CloudWatchLogs;
using Amazon.CloudWatchLogs.Model;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tweetinvi;
using Stream = Tweetinvi.Stream;
using TwitterCredentials = LogGenerator.Model.TwitterCredentials;

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

    public class TwitterStream {
        
        //--- Constants ---
        const string AWS_REGION = "us-west-2";
        const string LOG_GROUP = "/lambda-sharp/log-parser/dev";
        const string LOG_STREAM = "test-log-stream";
        
        //--- Methods ---
        public static void Run(int sampleSize = -1, string filterValue = "cats") {
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
                StartStreaming(client, sequenceToken, sampleSize, filterValue);
            } catch(Exception e) {
                Console.WriteLine(e);
            }
        }

        private static void StartStreaming(IAmazonCloudWatchLogs client, string sequenceToken, int sampleSize, string filterValue) {
            var counter = 0;
            var logEventsBatch = new List<InputLogEvent>();
            var stream = Stream.CreateFilteredStream();
            stream.AddTrack(filterValue);
            // stream.AddLocation(new Coordinates(-74, 40), new Coordinates(-73, 41));
            stream.MatchingTweetReceived += (sender, eventArgs) => {
                var json = eventArgs.Tweet.ToJson();
                Console.WriteLine(json);
                var tweetInfo = JsonConvert.DeserializeObject<JObject>(json);
                logEventsBatch.Add(new InputLogEvent {
                    Message = GetLogText(tweetInfo),
                    Timestamp = DateTime.Now
                });
                if(++counter % 10 == 0) {
                    sequenceToken = DispatchLogEvents(client, logEventsBatch, sequenceToken);
                    logEventsBatch = new List<InputLogEvent>();
                }
                if(--sampleSize == 0) {
                    sequenceToken = DispatchLogEvents(client, logEventsBatch, sequenceToken);
                    Console.WriteLine("Stoping TwitterStream");
                    stream.StopStream();
                }
            };
            stream.StartStreamMatchingAllConditions();
        }

        private static string GetLogText(JObject tweetInfo) {
            Console.WriteLine(tweetInfo["text"]);
            var user = tweetInfo["user"];
            var coordinates = tweetInfo["coordinates"];
            var retweetedStatus = tweetInfo["retweeted_status"];
            var entities = tweetInfo["entities"];
            var hashTags = entities["hashtags"];
            var text = new List<string> {
                $"[USER]: The user name is {user["name"]}', they have ({user["favourites_count"]}) favorite tweets and " +
                    $"have tweeted ({user["statuses_count"]}) times. " +
                    $"They have ({user["friends_count"]} )friends and follow ({user["followers_count"]}) people!!",
                $"[MESSAGE]: The tweet message is: ({tweetInfo["text"]})"
            };
            if(!retweetedStatus.IsNullOrEmpty()) {
                var retweetCount = retweetedStatus["retweet_count"];
                var favoriteCount = retweetedStatus["favorite_count"];
                text.Add($"[TWEET_INFO]: This tweet has been retweeted ({retweetCount}) times, and have been favorited by ({favoriteCount}) people");
            }
            if(!hashTags.IsNullOrEmpty()) {
                var hashTagsString = String.Join(", ", hashTags.Select(x => $"{x["text"]}"));
                text.Add($"[HASH_TAGS]: This tweet has the following hash tags: ({hashTagsString})");
            }
            if(!coordinates.IsNullOrEmpty()) {
                var latitude = coordinates["coordinates"][0];
                var longitude = coordinates["coordinates"][1];
                text.Add($"[LOCATION]: The location of this tweet is (lat: {latitude}, long: {longitude})");
            }
            return string.Join("\u03BB", text);
        }

        private static void SetTwitterCredentials() {

            // Set up twitter credentials (https://apps.twitter.com)
            var credentials = JsonConvert.DeserializeObject<TwitterCredentials>(File.ReadAllText("credentials.json"));    
            Auth.SetUserCredentials(credentials.ConsumerKey, credentials.ConsumerSecret, credentials.AccessToken, credentials.AccessSecret);
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
