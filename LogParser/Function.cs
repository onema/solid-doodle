using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using Amazon.CloudWatchLogs;
using Amazon.Lambda.Core;
using LogParser.Model;
using Newtonsoft.Json;

namespace LogParser {
    public class Function {
    
        //--- Fields ---
        private readonly IAmazonCloudWatchLogs _cloudWatchClient;
        private const string FILTER = @"^(\[[A-Z ]+\])";
        private static readonly Regex filter = new Regex(FILTER, RegexOptions.Compiled | RegexOptions.CultureInvariant); 
        
        //--- Constructors ---
        public Function() {
            _cloudWatchClient = new AmazonCloudWatchLogsClient();
        }

        //--- Methods ---
        [LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
        public void Handler(CloudWatchLogsEvent cloudWatchLogsEvent, ILambdaContext context) {
            // Level One
            Console.WriteLine($"THIS IS THE DATA: {cloudWatchLogsEvent.AwsLogs.Data}");
            var data = DecompressLogData(cloudWatchLogsEvent.AwsLogs.Data);
            Console.WriteLine($"THIS IS THE DECODED, UNCOMPRESSED DATA: {data}");
            var events = JsonConvert.DeserializeObject<DecompressedEvents>(data).LogEvents;
            var filteredEvents = events.Where(x => filter.IsMatch(x.Message)).ToList();
            filteredEvents.ForEach(x => Console.WriteLine(x.Message));
        }
        
        public static string DecompressLogData(string value) {
            var gzip = Convert.FromBase64String(value);
            using (GZipStream stream = new GZipStream(new MemoryStream(gzip), CompressionMode.Decompress)) 
            return new StreamReader(stream).ReadToEnd();
        }
    }
}
