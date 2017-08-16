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
using Tweetinvi.Streaming;
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
        
        //--- Fields ---
        private readonly IAmazonCloudWatchLogs _client;
        private readonly IFilteredStream _stream;
        
        //--- Properties ---
        string SequenceToken {
            get {
                var response = _client.DescribeLogStreamsAsync(new DescribeLogStreamsRequest {
                    LogGroupName = LOG_GROUP
                }).Result;
                return response.LogStreams.First(x => x.LogStreamName == LOG_STREAM).UploadSequenceToken;
            }
        }

        //--- Constructors ---
        public TwitterStream() {
            var region = RegionEndpoint.GetBySystemName(AWS_REGION);
            var chain = new CredentialProfileStoreChain();
            AWSCredentials awsCredentials;
            if(!chain.TryGetAWSCredentials("lambdasharp", out awsCredentials)) {
                throw new Exception("AWS Credentials not found!");
            }
            _client = new AmazonCloudWatchLogsClient(awsCredentials, region);
            SetTwitterCredentials();
            _stream = Stream.CreateFilteredStream();
        }
        
        //--- Methods ---
        public void Run(int sampleSize = -1, string filterValue = "cats") {
            try {
                StartStreaming(SequenceToken, sampleSize, filterValue);
            } catch(Exception e) {
                Console.WriteLine(e);
            }
        }

        private void StartStreaming(string sequenceToken, int sampleSize, string filterValue) {
            var counter = 0;
            var logEventsBatch = new List<InputLogEvent>();
            _stream.AddTrack(filterValue);
            // _stream.AddLocation(new Coordinates(32.0, -114.42), new Coordinates(41.96, -124.21));
            _stream.MatchingTweetReceived += (sender, eventArgs) => {
                Console.WriteLine(counter + ": " + eventArgs.Tweet);
                var json = eventArgs.Tweet.ToJson();
                var tweetInfo = JsonConvert.DeserializeObject<JObject>(json);
                logEventsBatch.Add(new InputLogEvent {
                    Message = GetLogText(tweetInfo),
                    Timestamp = DateTime.Now
                });
                if(++counter % 50 == 0) {
                    sequenceToken = DispatchLogEvents(logEventsBatch, sequenceToken);
                    logEventsBatch = new List<InputLogEvent>();
                }
                if(--sampleSize == 0) {
                    sequenceToken = DispatchLogEvents(logEventsBatch, sequenceToken);
                    Stop();
                }
            };
            _stream.StreamStopped += (sender, args) => {
                if(args.Exception != null) {
                    Console.WriteLine($"Stream stopped with exception: {args.Exception}");
                }
                if(args.DisconnectMessage != null) {
                    Console.WriteLine($"Disconnect message: {args.DisconnectMessage}");
                }
            };
            _stream.StartStreamMatchingAllConditions();
        }
        
        public string DispatchLogEvents(List<InputLogEvent> logEvents, string sequenceToken) {
            Console.WriteLine("Dispatching Log Events.");
            var request = new PutLogEventsRequest {
                  LogGroupName = LOG_GROUP,
                  LogStreamName = LOG_STREAM,
                  LogEvents = logEvents,
                  SequenceToken = sequenceToken
            };
            var response = _client.PutLogEventsAsync(request).Result;
            return response.NextSequenceToken;
        }    
        
        public void Stop() {
            Console.WriteLine("Stopping Stream...");
            _stream.StopStream();
        }

        private static string GetLogText(JObject tweetInfo) {
            // Console.WriteLine(tweetInfo["text"]);
            var user = tweetInfo["user"];
            var coordinates = tweetInfo["coordinates"];
            var retweetedStatus = tweetInfo["retweeted_status"];
            var entities = tweetInfo["entities"];
            var hashTags = entities["hashtags"];
            var tweetDate = tweetInfo["created_at"].ToString();
            var userDate = user["created_at"].ToString();
            var text = new List<string> {
                $"[USER]: The username is ({user["screen_name"]}) and their name is ({user["name"]}), they have ({user["favourites_count"]}) favorite tweets and " +
                    $"have tweeted ({user["statuses_count"]}) times. " +
                    $"They have ({user["friends_count"]}) friends and follow ({user["followers_count"]}) people!!" + 
                    $"This user was created on ({DateTime.Parse(userDate):yyyy-MM-dd HH:mm:ss})",
                $"[MESSAGE]: The tweet by ({user["screen_name"]}) is: ({tweetInfo["text"]}) and it was tweeted at ({DateTime.Parse(tweetDate):yyyy-MM-dd HH:mm:ss})"
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
                var latitude = coordinates["latitide"];
                var longitude = coordinates["longitude"];
                text.Add($"[LOCATION]: The location of this tweet is lat: ({latitude}), long: ({longitude})");
            }
            return string.Join("\u03BB", text);
        }

        private static void SetTwitterCredentials() {

            // Set up twitter credentials (https://apps.twitter.com)
            var credentials = JsonConvert.DeserializeObject<TwitterCredentials>(File.ReadAllText("credentials.json"));    
            Auth.SetUserCredentials(credentials.ConsumerKey, credentials.ConsumerSecret, credentials.AccessToken, credentials.AccessSecret);
        }
    }
}
