using System;
using System.Collections.Generic;
using System.Linq;
using Amazon;
using Amazon.CloudWatchLogs;
using Amazon.CloudWatchLogs.Model;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using CoreTweet;
using CoreTweet.Streaming;

namespace LogGenerator {
    
    //--- Class ---
    class Program {
    
        //--- Constants ---
        const string LOG_GROUP = "/lambda-sharp/log-parser/dev";
        const string LOG_STREAM = "test-log-stream";
        
        //--- Methods ---
        static void Main(string[] args) {
            var region = RegionEndpoint.GetBySystemName("us-west-2");
            var chain = new CredentialProfileStoreChain();
            AWSCredentials awsCredentials;
            if (chain.TryGetAWSCredentials("default", out awsCredentials)) { 
                var client = new AmazonCloudWatchLogsClient(awsCredentials, region);
                var sequenceToken = SequenceToken(client);
                StartStreaming(client, sequenceToken);
            }
        }

        private static void StartStreaming(IAmazonCloudWatchLogs client, string sequenceToken) {
            var counter = 0;
            var logEvents = new List<InputLogEvent> ();
            var tokens = GetTokens();
            var messages = tokens.Streaming.StartStreamAsync(
                StreamingType.Filter, 
                new StreamingParameters(new List<KeyValuePair<string, object>> {new KeyValuePair<string, object>("track", "tea")})
            ).Result;
            foreach (var streamingMessage in messages) {
                Console.WriteLine(streamingMessage.Json);
                logEvents.Add(new InputLogEvent {
                                          Message = streamingMessage.Json, 
                                          Timestamp = DateTime.Now
                                      });
                if(counter % 10 == 0) {
                    var request = new PutLogEventsRequest {
                              LogGroupName = LOG_GROUP,
                              LogStreamName = LOG_STREAM,
                              LogEvents = logEvents,
                              SequenceToken = sequenceToken
                        };
                        var response = client.PutLogEventsAsync(request).Result;
                        sequenceToken = response.NextSequenceToken;
                        logEvents = new List<InputLogEvent> ();
                }
            }
        }

        private static string SequenceToken(IAmazonCloudWatchLogs client) {
            var response = client.DescribeLogStreamsAsync(new DescribeLogStreamsRequest {
                LogGroupName = LOG_GROUP
            }).Result;
            
            return response.LogStreams.First().UploadSequenceToken;
        }

        private static Tokens GetTokens() {
            var consumerKey = "";
            var consumerSecret = "";
            var accessToken = "";
            var accessSecret = "";
            return Tokens.Create(consumerKey, consumerSecret, accessToken, accessSecret);
        }
    }
}
