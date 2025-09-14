using System;

namespace SeasonViewer.Core
{
    public static class Season
    {
        public static bool IsPlanToWatch(string season)
        {
            return string.Compare(season, "p2w", StringComparison.OrdinalIgnoreCase) == 0;
        }
    }
}
