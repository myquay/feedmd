using FeedMD.Infrastructure;
using Fluid;
using System.Reflection;

namespace FeedMD.Commands
{
    internal static class Run
    {
        internal static int Error()
        {
            Console.WriteLine("Unknown command, use 'help' command for how to use this utility");
            return 1;
        }

        internal static async Task<int> BuildOptions(BuildOptions opts)
        {
            var configuration = Configuration.Load(opts);

            if(configuration == null)
                return 1;

            Console.WriteLine($"Generating digest for {configuration.Date} ({configuration.TimeZone})");

            Directory.CreateDirectory(configuration.Destination);
            await using var fs = new FileStream($"{configuration.Destination}/{configuration.Date}.md", FileMode.Create);
            await using var sw = new StreamWriter(fs);

            var feedParser = new FeedParser(configuration);
            List<Feed> feeds = new();

            foreach (var feedUri in configuration.Feeds)
            {

                try
                {
                    feeds.Add(await feedParser.Parse(new Uri(feedUri)));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading feed {feedUri}: {ex.Message}");
                }
            }

            var parser = new FluidParser();
            if (parser.TryParse(configuration.Template, out var template, out var error))
            {

                var options = new TemplateOptions();
                options.MemberAccessStrategy.Register<Feed>();
                options.MemberAccessStrategy.Register<FeedItem>();

                var context = new TemplateContext(new
                {
                    configuration.Date,
                    Feeds = feeds.Where(f => f.Items.Any()).ToArray(),
                }, options);

                await sw.WriteAsync(await template.RenderAsync(context));
            }
            else
            {
                Console.WriteLine($"Error: {error}");
                return 1;
            }

            return 0;
        }

        internal static async Task<int> InitOptions(InitOptions opts)
        {
            await using var configStream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"feedmd.Data.{FileName.CONFIG}")!;
            await using var fs = new FileStream(FileName.CONFIG, FileMode.Create);
            await configStream.CopyToAsync(fs);

            await using var templateStream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"feedmd.Data.{FileName.TEMPLATE}")!;
            await using var ts = new FileStream(FileName.TEMPLATE, FileMode.Create);
            await templateStream.CopyToAsync(ts);

            return 0;
        }
    }
}
