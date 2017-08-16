using Newtonsoft.Json;

namespace LogGenerator.Model {
    
    //--- Classes ---
    public class ApplicationVars {

        //--- Properties ---
        [JsonProperty("log_group")]
        public string LogGroup { get; set; }
    
        [JsonProperty("log_stream")]
        public string LogStream { get; set; }
    }
}