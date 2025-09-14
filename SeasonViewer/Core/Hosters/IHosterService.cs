namespace SeasonViewer.Core.Hosters;

public interface IHosterService
{
    string GetHosterTypeFromUrl(string url);
}