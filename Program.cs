// See https://aka.ms/new-console-template for more information
using CommandLine;
using FeedMD.Infrastructure;
using System.Xml;
using System;

using Microsoft.SyndicationFeed;
using Microsoft.SyndicationFeed.Rss;
using Microsoft.SyndicationFeed.Atom;
using System.Text;
using System.Runtime.CompilerServices;
using System.Diagnostics;

var configuration = Configuration.Load(Environment.GetCommandLineArgs());
var client = new HttpClient();

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
        using var feedData = await client.GetAsync(feedUri, HttpCompletionOption.ResponseHeadersRead);

        if (feedData.Content.Headers.LastModified != null)
        {
            var lastModified = feedData.Content.Headers.LastModified.Value.UtcDateTime;

            if (lastModified < configuration.Start || lastModified > configuration.End)
            {
                if (configuration.Verbose)
                    Console.WriteLine($"Skipping {feedUri} as it was last modified at {lastModified} which is outside the digest period ({configuration.Start} - {configuration.End})");
                continue;
            }
        }

        using var reader = XmlReader.Create(await feedData.Content.ReadAsStreamAsync());

        while (reader.Read())
            if (reader.NodeType == XmlNodeType.Element)
                break;

        XmlFeedReader feedReader = reader.LocalName == "feed" ?
            new AtomFeedReader(reader) :
            new RssFeedReader(reader);

        var sb = new StringBuilder();
        sb.AppendLine();
        var containsRecentItems = false;
        var readingPastContent = false;


        while (await feedReader.Read() && !readingPastContent)
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
                    else if(item.Published.UtcDateTime < configuration.Start)
                    {
                        readingPastContent = true;
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
                    if(content.Name == "updated")
                    {
                        if(DateTime.TryParse(content.Value, out var lastModified))
                        {
                            if (lastModified < configuration.Start || lastModified > configuration.End)
                            {
                                if (configuration.Verbose)
                                    Console.WriteLine($"Skipping {feedUri} as it was last modified at {lastModified} which is outside the digest period ({configuration.Start} - {configuration.End})");
                                readingPastContent = true;
                            }
                        }
                    }
                    break;
            }
        }

        if (containsRecentItems)
            sw.WriteLine(sb.ToString());
    }
    catch (XmlException ex)
    {
        Console.WriteLine($"Error reading feed {feedUri}: {ex.Message}");
    }
}