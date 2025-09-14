using System.Collections.Generic;

namespace SeasonViewer.UserInterface;

public class AnimeDto
{
    public required long Id { get; set; }
    public string MalId { get; set; } = "";
    public string MalName { get; set; }= "";
    public string ImageId { get; set; }= "";
    public uint MalScore { get; set; }
    public ulong MalMembers { get; set; }
    public ulong MalEpisodesCount { get; set; }
    public ICollection<HosterDto> Hoster { get; set; } = [];
    public long HosterMinedAt { get; set; }
}
