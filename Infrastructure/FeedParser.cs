using Microsoft.SyndicationFeed.Atom;
using Microsoft.SyndicationFeed.Rss;
using Microsoft.SyndicationFeed;
using System.Xml;

namespace FeedMD.Infrastructure
{

    /// <summary>
    /// Parse a feed and return a list of items
    /// </summary>
    internal class FeedParser
    {
        private static readonly Lazy<HttpClient> Lazy = new(() =>
        {
            var client = new HttpClient();
            
            client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "feedmd/0.0.4");

            return client;
        });

        static HttpClient HttpClient => Lazy.Value;

        readonly Configuration _configuration;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configuration"></param>
        internal FeedParser(Configuration configuration)
        {
            this._configuration = configuration;
        }

        /// <summary>
        /// Parse a feed and return a list of items
        /// </summary>
        internal async Task<Feed> Parse(Uri feed)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, feed);
            request.Headers.IfModifiedSince = _configuration.Start;
            
            using var feedData = await HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            var parsedFeed = new Feed { Link = feed };

            using var reader = XmlReader.Create(await feedData.Content.ReadAsStreamAsync(), new XmlReaderSettings
            {
                DtdProcessing = _configuration.Strict ? DtdProcessing.Prohibit : DtdProcessing.Parse,
                MaxCharactersFromEntities = _configuration.MaxDtdCharacters,
                Async = true
            });

            while (await reader.ReadAsync())
                if (reader.NodeType == XmlNodeType.Element)
                    break;

            XmlFeedReader feedReader = reader.LocalName == "feed" ?
                new AtomFeedReader(reader) :
                new RssFeedReader(reader);

            while (await feedReader.Read())
            {
                switch (feedReader.ElementType)
                {
                    case SyndicationElementType.Item:
                        ISyndicationItem item = await feedReader.ReadItem();
                        var updated = item.Published.UtcDateTime == DateTime.MinValue ? item.LastUpdated.UtcDateTime : item.Published.UtcDateTime;

                        if (updated > _configuration.Start && updated < _configuration.End)
                        {
                            parsedFeed.Items.Add(new FeedItem
                            {
                                Title = string.IsNullOrEmpty(item.Title) ? $"{item.Links.First().Uri}" : item.Title,
                                Link = item.Links.First().Uri,
                                PublishDate = item.Published.UtcDateTime,
                                Content = item.Description
                            });
                        }
                        else if (updated < _configuration.Start)
                            return parsedFeed;
                        break;
                    case SyndicationElementType.Link:
                        ISyndicationLink link = await feedReader.ReadLink();
                        if (link.RelationshipType == "alternate")
                            parsedFeed.Link = link.Uri;
                        break;
                    default:
                        ISyndicationContent content = await feedReader.ReadContent();
                        if (content.Name == "title")
                            parsedFeed.Title = content.Value;
                        if (content.Name == "updated")
                        {
                            if (DateTime.TryParse(content.Value, out var lastModified))
                                parsedFeed.LastModified = lastModified;
                        }
                        break;
                }
            }

            return parsedFeed;
        }

    }

    /// <summary>
    /// Feed metadata
    /// </summary>
    internal class Feed
    {
        public string? Title { get; set; }

        public Uri? Link { get; set; }

        public DateTime LastModified { get; set; }

        public List<FeedItem> Items { get; set; } = new();
    }

    /// <summary>
    /// A feed item
    /// </summary>
    public class FeedItem
    {
        public string? Title { get; set; }
        public Uri? Link { get; set; }
        public string? Summary { get; set; }
        public string? Content { get; set; }
        public DateTime PublishDate { get; set; }
    }

}
