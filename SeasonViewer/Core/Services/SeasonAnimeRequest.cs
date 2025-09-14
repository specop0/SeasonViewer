namespace SeasonViewer.Core.Services;

public class SeasonAnimeRequest
{
    public string Name { get; set; } = "";
    public OrderCriteria OrderCriteria { get; set; }
    public GroupCriteria GroupCriteria { get; set; }
    public FilterCriteria FilterCriteria { get; set; }
}
