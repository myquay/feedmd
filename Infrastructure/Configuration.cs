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
using System.Reflection;
using System.Reflection.Metadata.Ecma335;

namespace FeedMD.Infrastructure
{
    class Configuration
    {
        public string TimeZone { get; set; } = "Pacific/Auckland";

        public bool Verbose { get; set; } = false;

        public DateTime Start { get; set; }

        public DateTime End { get; set; }

        public string Destination { get; set; } = "/";

        public string[] Feeds { get; set; } = Array.Empty<string>();

        public string Date
        {
            get
            {
                return Instant.FromDateTimeUtc(Start)
                    .InZone(DateTimeZoneProviders.Tzdb.GetZoneOrNull(TimeZone) ?? DateTimeZoneProviders.Tzdb.GetSystemDefault())
                    .Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            }
        }

        public string? Template { get; set; }

        public static Configuration? Load(BuildOptions opts)
        {
            //Defaults
            var configuration = new Configuration();

            var yamlOptions = File.Exists(opts.Configuration) ? new DeserializerBuilder()
                 .WithNamingConvention(UnderscoredNamingConvention.Instance)  // see height_in_inches in sample yml 
                 .Build().Deserialize<YamlOptions>(File.ReadAllText(opts.Configuration)) : null;

            if(yamlOptions == null)
            {
                Console.WriteLine($"Cannot load configuration at '{opts.Configuration}'");
                return null;
            }

            configuration.Template = File.Exists(opts.Template) ? File.ReadAllText(opts.Template) :
                new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream($"feedmd.Data.{FileName.TEMPLATE}")!).ReadToEnd();

            //Set Time Zone
            configuration.TimeZone = opts?.TimeZone ?? yamlOptions.TimeZone ?? configuration.TimeZone;

            var zone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(configuration.TimeZone) ?? DateTimeZoneProviders.Tzdb.GetSystemDefault();
            var date = Instant.FromDateTimeUtc(DateTime.UtcNow)
                .InZone(zone).Date.PlusDays(-1);

            //Set the date of the digest
            if (opts?.Date != null)
            {
                date = LocalDatePattern.CreateWithInvariantCulture("yyyy-MM-dd")
                    .Parse(opts.Date)
                    .GetValueOrThrow();

            }

            //Set start/ end dates
            configuration.Start = date.AtStartOfDayInZone(zone).ToDateTimeUtc();
            configuration.End = date.PlusDays(1).AtStartOfDayInZone(zone).ToDateTimeUtc();

            //Set destination
            configuration.Destination = opts?.Destination ?? configuration.Destination;

            //Set feeds
            configuration.Feeds = yamlOptions.Feeds ?? configuration.Feeds;

            //Set verbose
            configuration.Verbose = opts?.Verbose ?? configuration.Verbose;

            return configuration;
        }
    }
}
