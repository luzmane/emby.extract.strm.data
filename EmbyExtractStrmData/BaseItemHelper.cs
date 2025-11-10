using System.Collections.Generic;
using System.Linq;

using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Entities;

namespace EmbyExtractStrmData
{
    public class BaseItemHelper
    {
        public static bool HasBothMediaStreams(BaseItem item)
        {
            var streams = item.GetMediaStreams() ?? new List<MediaStream>();
            return streams.Any(s => s.Type == MediaStreamType.Video)
                   && streams.Any(s => s.Type == MediaStreamType.Audio);
        }
    }
}
