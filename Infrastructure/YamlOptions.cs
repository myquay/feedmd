using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedMD.Infrastructure
{
    class YamlOptions
    {
        public string? TimeZone { get; set; }

        public string[] Feeds { get; set; } = Array.Empty<string>();

    }
}
