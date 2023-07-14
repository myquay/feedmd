using CommandLine.Text;
using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.Design;

namespace FeedMD.Infrastructure
{
    [Verb(Command.INIT, HelpText = "Create default config.yml, and template.tmd files")]
    class InitOptions
    {

    }

    [Verb(FeedMD.Command.BUILD, HelpText = "Build the MD file from feeds")]
    class BuildOptions
    {

        public string? Command { get; set; }

        [Option('v', "verbose",
            Default = false,
            HelpText = "Prints all messages to standard output.")]
        public bool Verbose { get; set; }

        [Option("date",
            HelpText = "Date to generate the digest for")]
        public string? Date { get; set; }

        [Option('d', "destination",
            Default = "./",
            HelpText = "Where to save the digest markdown file to")]
        public string? Destination { get; set; }

        [Option("tz",
            HelpText = "Time zone for the digest")]
        public string? TimeZone { get; set; }

        [Option('c', "configuration",
           Default = "./config.yml",
           HelpText = "Where to load the configuration from")]
        public string? Configuration { get; set; }

        [Option('t', "template",
           Default = "./template.tmd",
           HelpText = "Where to load the .tmd template from")]
        public string? Template { get; set; }
    }
}
