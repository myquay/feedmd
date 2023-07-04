using CommandLine.Text;
using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodaTime.Text;
using NodaTime;
using System.Globalization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;

namespace FeedMD.Infrastructure
{
    class Configuration
    {
        public string TimeZone { get; set; } = "Pacific/Auckland";

        public bool Verbose { get; set; } = false;

        public DateTime Start { get; set; }

        public DateTime End { get; set; }

        public string Destination { get; set; } = "/";

        public string[] Feeds { get; set; } = new string[] { };

        public string Date
        {
            get
            {
                return Instant.FromDateTimeUtc(Start)
                    .InZone(DateTimeZoneProviders.Tzdb.GetZoneOrNull(TimeZone) ?? DateTimeZoneProviders.Tzdb.GetSystemDefault())
                    .Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            }
        }

        public static Configuration Load(string[] args)
        {
            //Defaults
            var configuration = new Configuration();

            //Load configuration
            var cmdOptions = CommandLine.Parser.Default.ParseArguments<CmdOptions>(Environment.GetCommandLineArgs()).Value;
            var yamlOptions = new DeserializerBuilder()
                 .WithNamingConvention(UnderscoredNamingConvention.Instance)  // see height_in_inches in sample yml 
                 .Build().Deserialize<YamlOptions>(File.ReadAllText("config.yaml"));

            //Set Time Zone
            configuration.TimeZone = cmdOptions?.TimeZone ?? yamlOptions.TimeZone ?? configuration.TimeZone;

            var zone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(configuration.TimeZone) ?? DateTimeZoneProviders.Tzdb.GetSystemDefault();
            var date = Instant.FromDateTimeUtc(DateTime.UtcNow)
                .InZone(zone).Date.PlusDays(-1);

            //Set the date of the digest
            if (cmdOptions?.Date != null)
            {
                date = LocalDatePattern.CreateWithInvariantCulture("yyyy-MM-dd")
                    .Parse(cmdOptions.Date)
                    .GetValueOrThrow();

            }

            //Set start/ end dates
            configuration.Start = date.AtStartOfDayInZone(zone).ToDateTimeUtc();
            configuration.End = date.PlusDays(1).AtStartOfDayInZone(zone).ToDateTimeUtc();

            //Set destination
            configuration.Destination = cmdOptions?.Destination ?? configuration.Destination;

            //Set feeds
            configuration.Feeds = yamlOptions.Feeds ?? configuration.Feeds;

            //Set verbose
            configuration.Verbose = cmdOptions?.Verbose ?? configuration.Verbose;

            return configuration;
        }
    }
}
