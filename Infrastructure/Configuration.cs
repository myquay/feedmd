using NodaTime.Text;
using NodaTime;
using System.Globalization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using System.Reflection;

namespace FeedMD.Infrastructure
{
    class Configuration
    {
        public string TimeZone { get; private set; } = "Pacific/Auckland";

        public bool Verbose { get; set; }

        public DateTime Start { get; private set; }

        public DateTime End { get; private set; }

        public string Destination { get; private set; } = "/";

        public string[] Feeds { get; private set; } = Array.Empty<string>();

        public string Date =>
            Instant.FromDateTimeUtc(Start)
                .InZone(DateTimeZoneProviders.Tzdb.GetZoneOrNull(TimeZone) ?? DateTimeZoneProviders.Tzdb.GetSystemDefault())
                .Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        public string? Template { get; private set; }

        public bool Strict { get; private set; }
        
        public int MaxDtdCharacters { get; private set; }
        
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
            configuration.TimeZone = opts.TimeZone ?? yamlOptions.TimeZone ?? configuration.TimeZone;

            var zone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(configuration.TimeZone) ?? DateTimeZoneProviders.Tzdb.GetSystemDefault();
            var date = Instant.FromDateTimeUtc(DateTime.UtcNow)
                .InZone(zone).Date.PlusDays(-1);

            //Set the date of the digest
            if (opts.Date != null)
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

            //Set parsing strictness
            configuration.Strict = opts?.Strict ?? configuration.Strict;
            
            //Set max DTD characters
            configuration.MaxDtdCharacters = opts?.MaxDtdCharacters ?? configuration.MaxDtdCharacters;
            
            return configuration;
        }
    }
}
