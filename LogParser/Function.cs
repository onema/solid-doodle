using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;
using LogParser.Model;
using Newtonsoft.Json;

namespace LogParser {
    
    //--- Classes ---
    public static class StringEx {
        
        //--- Methods ---
        public static bool IsUser(this string value) {
            return value.StartsWith("[USER]:");
        }
        public static bool IsMessage(this string value) {
            return value.StartsWith("[MESSAGE]:");
        }
        public static bool IsTweetInfo(this string value) {
            return value.StartsWith("[TWEET_INFO]:");
        }
        public static bool IsHashtags(this string value) {
            return value.StartsWith("[HASH_TAGS]:");
        }
        public static bool IsLocation(this string value) {
            return value.StartsWith("[LOCATION]:");
        }
    }
    
    public class Function {
    
        //--- Fields ---
        private readonly string logsBucket = Environment.GetEnvironmentVariable("LOGS_BUCKET");
        private readonly IAmazonS3 _s3Client;
        
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
            var framedEvents = events.Select(x => x.Message.Split('\u03BB').ToList()).ToList();
            var userJson = UserJson(framedEvents);
            var tweetJson = TweetJson(framedEvents);
            
            // Level 3: Save data to S3
            PutObject(userJson, "users");
            PutObject(tweetJson, "tweet-info");

            // Level 4: Create athena schema to query data
        }

        public void PutObject(IEnumerable<string> values, string type) {
            if (values.Any()) {
                _s3Client.PutObjectAsync(
                    new PutObjectRequest() {
                        BucketName = logsBucket,
                        Key = $"{type}/{Guid.NewGuid()}",
                        ContentBody = string.Join("\n", values.Select(x => x.Replace(Environment.NewLine, "")).ToArray())
                    }
                ).Wait();
            }
        }
        
        public static string DecompressLogData(string value) {
            var gzip = Convert.FromBase64String(value);
            using (GZipStream stream = new GZipStream(new MemoryStream(gzip), CompressionMode.Decompress)) 
            return new StreamReader(stream).ReadToEnd();
        }

        private static IEnumerable<string> UserJson(IEnumerable<List<string>> framedEvents) {
            var userJson = framedEvents.Select(x => {
                var user = x.FirstOrDefault(y => y.IsUser()) ?? "";
                return JsonConvert.SerializeObject(new User(user));
            }).ToList();
            return userJson;
        }

        public static IEnumerable<string> TweetJson(IEnumerable<List<string>> framedEvents) {
            var tweetJson = framedEvents.Select(x => {
                var message = x.FirstOrDefault(y => y.IsMessage()) ?? "";
                var tweetInfo = x.FirstOrDefault(y => y.IsTweetInfo()) ?? "";
                var hashTags = x.FirstOrDefault(y => y.IsHashtags()) ?? "";
                var location = x.FirstOrDefault(y => y.IsLocation()) ?? "";
                return JsonConvert.SerializeObject(new TweetInfo(message, tweetInfo, hashTags, location));
            }).ToList();
            return tweetJson;
        }
    }
}
