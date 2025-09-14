using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SeasonViewer.Core;
using SeasonViewer.Core.Hosters;
using SeasonViewer.Core.Services;

namespace SeasonViewer.UserInterface
{
    public class AnimeSeasonService
    {
        public AnimeSeasonService(SeasonService client, IHosterService hosterService)
        {
            this.Client = client;
            this.HosterService = hosterService;
        }

        private SeasonService Client { get; }
        public IHosterService HosterService { get; }

        public async Task<ICollection<AnimeDto>> GetSeasonAsync(string? request, OrderCriteria orderBy, GroupCriteria groupBy, FilterCriteria filterBy)
        {
            var seasonAnimeRequest = this.CreateSeasonAnimeRequest(request, orderBy, groupBy, filterBy);
            var animes = await this.Client.GetSeasonAsync(seasonAnimeRequest);
            return [.. animes.Select(this.Convert)];
        }

        public async Task<ICollection<AnimeDto>> UpdateSeasonAsync(string? request, OrderCriteria orderBy, GroupCriteria groupBy, FilterCriteria filterBy)
        {
            var seasonAnimeRequest = this.CreateSeasonAnimeRequest(request, orderBy, groupBy, filterBy);
            var animes = await this.Client.UpdateSeasonAsync(seasonAnimeRequest);
            return [.. animes.Select(this.Convert)];
        }

        public async Task<ICollection<AnimeDto>> UpdateMalListAsync(string? request, OrderCriteria orderBy, GroupCriteria groupBy, FilterCriteria filterBy)
        {
            var seasonAnimeRequest = this.CreateSeasonAnimeRequest(request, orderBy, groupBy, filterBy);
            var animes = await this.Client.UpdateMalListAsync(seasonAnimeRequest);
            return [.. animes.Select(this.Convert)];
        }

        private SeasonAnimeRequest CreateSeasonAnimeRequest(string? request, OrderCriteria orderBy, GroupCriteria groupBy, FilterCriteria filterBy)
        {
            var seasonAnimeRequest = new SeasonAnimeRequest();
            seasonAnimeRequest.Name = request ?? ""; ;
            seasonAnimeRequest.OrderCriteria = Enum.Parse<Core.Services.OrderCriteria>(orderBy.ToString());
            seasonAnimeRequest.GroupCriteria = Enum.Parse<Core.Services.GroupCriteria>(groupBy.ToString());
            seasonAnimeRequest.FilterCriteria = Enum.Parse<Core.Services.FilterCriteria>(filterBy.ToString());
            return seasonAnimeRequest;
        }

        public async Task<AnimeDto> MineMalAsync(long id)
        {
            var anime = await this.Client.MineMalAsync(id);
            return this.Convert(anime);
        }

        public async Task<AnimeDto> MineHosterAsync(long id)
        {
            var anime = await this.Client.MineHosterAsync(id);
            return this.Convert(anime);
        }

        public async Task<AnimeDto> EditHosterAsync(long id, ICollection<HosterDto> hosters)
        {
            var response = await this.Client.EditHosterAsync(id, [.. hosters.Select(this.Convert)]);

            return this.Convert(response);
        }

        private AnimeDto Convert(Anime anime)
        {
            var seasonAnime = new AnimeDto
            {
                Id = anime.Id,
            };

            if (anime.Mal != null)
            {
                seasonAnime.MalId = anime.Mal.Id;
                seasonAnime.MalName = anime.Mal.Name ?? string.Empty; ;
                seasonAnime.ImageId = string.Empty;
                if (!string.IsNullOrEmpty(anime.Mal.ImageUrl))
                {
                    seasonAnime.ImageId = System.Convert.ToBase64String(Encoding.UTF8.GetBytes(anime.Mal.ImageUrl));
                }
                seasonAnime.MalScore = anime.Mal.Score;
                seasonAnime.MalMembers = anime.Mal.MemberCount;
                seasonAnime.MalEpisodesCount = anime.Mal.EpisodesCount;
            }

            if (anime.HosterMinedAt.HasValue)
            {
                seasonAnime.HosterMinedAt = anime.HosterMinedAt.Value.Ticks;
            }

            var hosters = anime.Hoster.Select(x =>
            {
                var hoster = new HosterDto
                {
                    Name = x.Name ?? string.Empty,
                    HosterType = this.HosterService.GetHosterTypeFromUrl(x.Url),
                    Url = x.Url ?? string.Empty
                };

                return hoster;
            });
            seasonAnime.Hoster = [.. hosters];

            return seasonAnime;
        }

        private Hoster Convert(HosterDto hosterDto)
        {
            return new Hoster
            {
                Name = hosterDto.Name,
                Url = hosterDto.Url,
            };
        }

        public async Task<ImageDataDto> GetImageDataAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new ImageDataDto
                {
                    Data = [],
                    MimeType = "",
                };
            }

            var response = await this.Client.GetImageDataAsync(id);

            return new ImageDataDto
            {
                Data = response.Data,
                MimeType = response.MimeType,
            };
        }
    }
}
