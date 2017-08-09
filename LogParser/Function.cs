using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using Amazon.CloudWatchLogs;
using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;
using LogParser.Model;
using Newtonsoft.Json;

namespace LogParser {
    public class Function {
        
        //--- Constants ---
        private const string FILTER = @"^(\[[A-Z ]+\])";
    
        //--- Fields ---
        private readonly string logsBucket = Environment.GetEnvironmentVariable("LOGS_BUCKET");
        private readonly IAmazonCloudWatchLogs _cloudWatchClient;
        private readonly IAmazonS3 _s3Client;
        private static readonly Regex filter = new Regex(FILTER, RegexOptions.Compiled | RegexOptions.CultureInvariant); 
        
        //--- Constructors ---
        public Function() {
            _cloudWatchClient = new AmazonCloudWatchLogsClient();
            _s3Client = new AmazonS3Client();
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
            filteredEvents.ForEach(x => Console.WriteLine($"MESSAGE: {x.Message}"));
            _s3Client.PutObjectAsync(
                new PutObjectRequest(){
                    BucketName = logsBucket,
                    Key = Guid.NewGuid().ToString(),
                    ContentBody = string.Join("\n", filteredEvents.Select(x => x.Message).ToArray())
                }
            ).Wait();
        }
        
        public static string DecompressLogData(string value) {
            var gzip = Convert.FromBase64String(value);
            using (GZipStream stream = new GZipStream(new MemoryStream(gzip), CompressionMode.Decompress)) 
            return new StreamReader(stream).ReadToEnd();
        }
    }
}
