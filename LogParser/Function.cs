using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;
using LogParser.Model;
using Newtonsoft.Json;

namespace LogParser {
    public class Function {
        
        //--- Constants ---
        private const string FILTER = @"(\[[\w ]+\])";
    
        //--- Fields ---
        private readonly string logsBucket = Environment.GetEnvironmentVariable("LOGS_BUCKET");
        private readonly IAmazonS3 _s3Client;
        private static readonly Regex filter = new Regex(FILTER, RegexOptions.Compiled | RegexOptions.CultureInvariant); 
        
        //--- Constructors ---
        public Function() {
            _s3Client = new AmazonS3Client();
        }

        //--- Methods ---
        [LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
        public void Handler(CloudWatchLogsEvent cloudWatchLogsEvent, ILambdaContext context) {
            // Level 1: decode and decompress data
            Console.WriteLine($"THIS IS THE DATA: {cloudWatchLogsEvent.AwsLogs.Data}");
            var data = DecompressLogData(cloudWatchLogsEvent.AwsLogs.Data);
            Console.WriteLine($"THIS IS THE DECODED, UNCOMPRESSED DATA: {data}");
            
            // Level 2: Frame and filter events
            var events = JsonConvert.DeserializeObject<DecompressedEvents>(data).LogEvents;
            List<string> framedEvents = events.Select(x => x.Message.Split('\u03BB').ToList())
                .SelectMany(i => i)
                .Distinct()
                .ToList();
            framedEvents.ForEach(x => Console.WriteLine($"FRAMED MESSAGE: {x}"));
            var filteredEvents = framedEvents.Where(x => filter.IsMatch(x)).ToList();
            filteredEvents.ForEach(x => Console.WriteLine($"MESSAGE: {x}"));
            
            // Level 3: Save data to S3
            _s3Client.PutObjectAsync(
                new PutObjectRequest(){
                    BucketName = logsBucket,
                    Key = Guid.NewGuid().ToString(),
                    ContentBody = string.Join("\n", filteredEvents.Select(x => x).ToArray())
                }
            ).Wait();
            
            // Level 4: Create athena schema to query data
        }
        
        public static string DecompressLogData(string value) {
            var gzip = Convert.FromBase64String(value);
            using (GZipStream stream = new GZipStream(new MemoryStream(gzip), CompressionMode.Decompress)) 
            return new StreamReader(stream).ReadToEnd();
        }
    }
}
