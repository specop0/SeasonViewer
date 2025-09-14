using System.Collections.Generic;
using System.Threading.Tasks;

namespace SeasonViewer.Core.Miner;

public interface ISeleniumMiner
{
    Task<ICollection<Anime>> MineAnimesAsync(ICollection<Anime> animes);
    Task<ICollection<Hoster>> MineHosterAsync(Anime anime);
    Task<byte[]> MineImageAsync(string imageUrl);
    Task<MyAnimeList?> MineMalAsync(Anime anime);
    Task<ICollection<MalListMineResult>> MineMalListAsync(string user);
    Task<ICollection<Anime>> MineSeasonAnimeAsync(string season);
}