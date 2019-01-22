using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Video.Indexer.Models
{
    public class VideoModel
    {
        public string Search { get; set; }
        public Uri InsightsWidgetUri { get; set; }
        public Uri PlayerWidgetUri { get; set; }
    }
}
