namespace AspireAARedis.Web;

public class LeaderboardApiClient(HttpClient httpClient)
{
    public async Task<Player[]> GetLeaderboardAsync(int maxItems = 10, CancellationToken cancellationToken = default)
    {
        List<Player>? players = null;

        await foreach (var player in httpClient.GetFromJsonAsAsyncEnumerable<Player>("/leaderboard", cancellationToken))
        {
            if (players?.Count >= maxItems)
            {
                break;
            }
            if (player is not null)
            {
                players ??= [];
                players.Add(player);
            }
        }

        return players?.ToArray() ?? [];
    }

    public async Task PostPlayerAsync(string name, CancellationToken cancellationToken = default)
    {
        await httpClient.PostAsJsonAsync($"/players/{name}", new Player(name, new Random().Next(0, 1000)), cancellationToken);
    }
}

public record Player(string Name, int Score);

