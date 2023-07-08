// See https://aka.ms/new-console-template for more information
using CommandLine;
using FeedMD.Infrastructure;
using System.Xml;
using System;

using Microsoft.SyndicationFeed;
using Microsoft.SyndicationFeed.Rss;
using Microsoft.SyndicationFeed.Atom;
using System.Text;

var configuration = Configuration.Load(Environment.GetCommandLineArgs());

Console.WriteLine($"Generating digest for {configuration.Date} ({configuration.TimeZone})");

Directory.CreateDirectory(configuration.Destination);
using var fs = new FileStream($"{configuration.Destination}/{configuration.Date}.md", FileMode.Create);
using var sw = new StreamWriter(fs);

//Write out the preamble
sw.WriteLine("---");
sw.WriteLine($"publishDate: {configuration.Date}");
sw.WriteLine($"title: 'RSS Daily Digest: {configuration.Date}'");
sw.WriteLine($"url: /digest/{configuration.Date}");
sw.WriteLine("---");

foreach (var feedUri in configuration.Feeds)
{
    try
    {
        using var reader = XmlReader.Create(feedUri, new XmlReaderSettings() { Async = true });

        while (reader.Read())
            if (reader.NodeType == XmlNodeType.Element)
                break;

        XmlFeedReader feedReader = reader.LocalName == "feed" ?
            new AtomFeedReader(reader) :
            new RssFeedReader(reader);

        var sb = new StringBuilder();
        sb.AppendLine();
        bool containsRecentItems = false;

        while (await feedReader.Read())
        {
            switch (feedReader.ElementType)
            {
                case SyndicationElementType.Item:
                    ISyndicationItem item = await feedReader.ReadItem();
                    if (item.Published.UtcDateTime > configuration.Start && item.Published < configuration.End)
                    {
                        containsRecentItems = true;
                        sb.AppendLine($"* [{item.Title}]({item.Links.First().Uri})");
                    }
                    break;
                case SyndicationElementType.Link:
                    ISyndicationLink link = await feedReader.ReadLink();
                    if (link.RelationshipType == "alternate")
                        sb.AppendLine($"{link.Uri})\n");
                    break;
                default:
                    ISyndicationContent content = await feedReader.ReadContent();
                    if (content.Name == "title")
                        sb.Append($"### [{content.Value}](");
                    break;
            }
        }

        if (containsRecentItems)
        {
            sw.WriteLine(sb.ToString());
        }
    }
    catch (XmlException ex)
    {
        Console.WriteLine($"Error reading feed {feedUri}: {ex.Message}");
    }
}