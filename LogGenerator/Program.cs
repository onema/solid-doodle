using System;
using System.Collections.Generic;
using Amazon;
using Amazon.CloudWatchLogs;
using Amazon.CloudWatchLogs.Model;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;

namespace LogGenerator {

    //--- Class ---
    class Program {
    
        //--- Methods ---
        static void Main(string[] args) {
            var region = RegionEndpoint.GetBySystemName("us-west-2");
            var chain = new CredentialProfileStoreChain();
            AWSCredentials awsCredentials;
            if (chain.TryGetAWSCredentials("default", out awsCredentials)) { 
                var client = new AmazonCloudWatchLogsClient(awsCredentials, region);
                var request = new PutLogEventsRequest {
                      LogGroupName = "juant/lambda/log-parser",
                      LogStreamName = "Test1",
                      LogEvents = new List<InputLogEvent> {
                          new InputLogEvent {Message = "Foo", Timestamp = DateTime.Now}
                          
                      },
                      SequenceToken = "49561000940692882142799072899892036996907997445775957554"
                };
                
                var response = client.PutLogEventsAsync(request).Result;
                Console.WriteLine(response.NextSequenceToken);
            }
        }
    }
}
