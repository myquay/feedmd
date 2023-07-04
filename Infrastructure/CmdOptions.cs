using CommandLine.Text;
using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedMD.Infrastructure
{
    class CmdOptions
    {
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

    }
}
