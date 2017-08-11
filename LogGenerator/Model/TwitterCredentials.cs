using Newtonsoft.Json;

namespace LogGenerator.Model {
    
    //--- Classes ---
    public class TwitterCredentials {
        
        //--- Properties ---
        [JsonProperty("consumer_key")]
        public string ConsumerKey { get; set; }
        
        [JsonProperty("consumer_secret")]
        public string ConsumerSecret { get; set; }
        
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        
        [JsonProperty("access_secret")]
        public string AccessSecret { get; set; }
    }
}