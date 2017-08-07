using System;
using System.Collections.Generic;
using Amazon;
using Amazon.CloudWatchLogs;
using Amazon.CloudWatchLogs.Model;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Tweetinvi;
using System.Linq;

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
            if(!chain.TryGetAWSCredentials("default", out awsCredentials)) {
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
                logEventsBatch.Add(new InputLogEvent {
                    Message = eventArgs.Tweet.ToJson(),
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
            var consumerKey = "";
            var consumerSecret = "";
            var accessToken = "";
            var accessSecret = "";
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
