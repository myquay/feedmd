namespace FeedMD.Infrastructure
{
    class YamlOptions
    {
        public string? TimeZone { get; set; }

        public string[] Feeds { get; set; } = Array.Empty<string>();

    }
}
