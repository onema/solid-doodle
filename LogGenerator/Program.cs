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
                var request = new PutLogEventsRequest {
                      LogGroupName = LOG_GROUP,
                      LogStreamName = LOG_STREAM,
                      LogEvents = new List<InputLogEvent> {
                          new InputLogEvent {
                              Message = "Test1\n", 
                              Timestamp = DateTime.Now
                          },
                          new InputLogEvent {
                              Message = "Error: foobar\n",
                              Timestamp = DateTime.Now
                          },
                          new InputLogEvent {
                              Message = "[ERROR] Foo\n",
                              Timestamp = DateTime.Now
                          },
                          new InputLogEvent {
                              Message = "Error: foobar\n",
                              Timestamp = DateTime.Now
                          },
                          new InputLogEvent {
                              Message = "[WARNING] Bar\n",
                              Timestamp = DateTime.Now
                          },
                          new InputLogEvent {
                              Message = "Error: foobar\n",
                              Timestamp = DateTime.Now
                          },
                          new InputLogEvent {
                              Message = "[Info] Should not show up\n",
                              Timestamp = DateTime.Now
                          },
                          new InputLogEvent {
                              Message = "[INFO] this should show up\n",
                              Timestamp = DateTime.Now
                          },
                          new InputLogEvent {
                              Message = "[BlA!]\n",
                              Timestamp = DateTime.Now
                          },
                          new InputLogEvent {
                              Message = "[BALH]\n",
                              Timestamp = DateTime.Now
                          },
                          new InputLogEvent {
                              Message = "[Soccer]\n",
                              Timestamp = DateTime.Now
                          },
                          new InputLogEvent {
                              Message = "[SOCCER] alalalalala goool\n",
                              Timestamp = DateTime.Now
                          },
                          new InputLogEvent {
                              Message = "ajk",
                               Timestamp = DateTime.Now
                          }
                      },
//                      SequenceToken = "49567750446880921213636912228400127364839351295405982594"
                };
                
                var response = client.PutLogEventsAsync(request).Result;
                Console.WriteLine(response.NextSequenceToken);
            }
        }
    }
}
