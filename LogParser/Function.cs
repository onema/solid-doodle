using System;
using Amazon.CloudWatchLogs;
using Amazon.Lambda.Core;

namespace LogParser {
    
    //--- Classes ---
    public class CloudWatchLogsEvent {
    
        //--- Properties ---
        public Awslogs awslogs { get; set; }
    }
    
    public class Awslogs {
        
        //--- Properties ---
        public string data { get; set; }
    }

    public class Function {
    
        //--- Fields ---
        private readonly IAmazonCloudWatchLogs _cloudWatchClient;

        //--- Methods ---
        public Function() {
            _cloudWatchClient = new AmazonCloudWatchLogsClient();
        }

        [LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
        public void Handler(CloudWatchLogsEvent cloudWatchLogsEvent, ILambdaContext context) {
            Console.WriteLine(cloudWatchLogsEvent.awslogs.data);
        }
    }
}
