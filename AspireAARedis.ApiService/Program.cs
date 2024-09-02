using AspireAARedis.ApiService;
using StackExchange.Redis;
using System.ComponentModel.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();
// Add Redis cache
builder.Services.AddSingleton(async x => await RedisConnection.InitializeAsync("cache"));

// Add services to the container.
builder.Services.AddProblemDetails();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

// Get Redis connection
var _redisConnectionFactory = app.Services.GetRequiredService<Task<RedisConnection>>();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
});

app.MapGet("/leaderboard", async () =>
{
    RedisConnection _redisConnection = await _redisConnectionFactory;

    var players = await _redisConnection.BasicRetryAsync(async (db) => (await db.SortedSetRangeByRankWithScoresAsync("gameScoreSortedSet", order: Order.Descending))
            .Select(p => new Player
            {
                Name = p.Element,
                Score = (int)p.Score
            })
            .ToList()
            );

    return players;
});

app.MapPost("/players/{name}", async (string name) =>
{
    Random rand = new Random();
    Player player = new Player
    { 
        Name = name,
        Score = rand.Next(0, 1000)
    };

    RedisConnection _redisConnection = await _redisConnectionFactory;

    await _redisConnection.BasicRetryAsync(async (db) => await db.SortedSetUpdateAsync("gameScoreSortedSet", player.Name, player.Score));
});

app.MapDefaultEndpoints();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

public class Player
{
    public required string Name { get; set; }
    public int Score { get; set; }

}
