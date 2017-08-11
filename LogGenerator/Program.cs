using System;
using Microsoft.Extensions.CommandLineUtils;

namespace LogGenerator {

    class Program {

        //--- Methods ---
        static void Main(string[] args) {
            var twitterStream = new TwitterStream();
            var app = new CommandLineApplication();
            Console.CancelKeyPress += delegate {
                // call methods to clean up
                twitterStream.Stop();
            };
            var generate = app.Command("generate", config => { 
                config.OnExecute(()=>{
                    config.ShowHelp(); //show help for generate
                    return 1; //return error since we didn't do anything
                });
                config.HelpOption("-? | -h | --help"); //show help on --help
            });
            generate.Command("help", config => { 
                 config.Description = "get help!";
                 config.OnExecute(()=>{
                    generate.ShowHelp("generate");
                     return 1;
                 });
             });
            generate.Command("logs", config => {
                config.Description = "generate logs using the twitter stream API";
                config.HelpOption("-? | -h | --help");
                var filterValue = config.Argument("filter", "value for the filter, e.g. cats, baseball, news", false);
                config.OnExecute(()=>{ 
                    twitterStream.Run(-1, filterValue.Value);
                    return 0;
                });   
            });
            generate.Command("sample", config => {
                config.Description = "generate a small sample of logs using the twitter stream API";
                config.HelpOption("-? | -h | --help");
                var number = config.Argument("number", "number of tweets to use to generate logs", false);
                var filterValue = config.Argument("filter", "value for the filter, e.g. cats, baseball, news", false);
                config.OnExecute(()=>{
                    if (string.IsNullOrWhiteSpace(number.Value)) return 1;
                    twitterStream.Run(int.Parse(number.Value), filterValue.Value);
                    return 0;
                });   
            });
            app.HelpOption("-? | -h | --help");
            var result = app.Execute(args);
            Environment.Exit(result);
        }
    }
}
