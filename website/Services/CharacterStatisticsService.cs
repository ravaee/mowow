using MySql.Data.MySqlClient;
using Dapper;
using AzerothCoreIntegration.Models;

namespace AzerothCoreIntegration.Services;

public class CharacterStatisticsService(IConfiguration cfg)
{
    private readonly string _conn = cfg.GetConnectionString("AzerothCoreCharactersDatabase");

    public async Task<FactionStatisticsModel> GetFactionStatisticsAsync()
    {
        using var db = new MySqlConnection(_conn);
        await db.OpenAsync();

        const string sql = """
            SELECT race, COUNT(*) AS c
            FROM characters
            WHERE online = 1
            GROUP BY race;
            """;

        var rows = await db.QueryAsync<dynamic>(sql);

        int horde = 0, alliance = 0;

        foreach (var r in rows)
        {
            int race = r.race;
            int count = Convert.ToInt32(r.c);

            if (IsHorde(race)) horde += count;
            else if (IsAlliance(race)) alliance += count;
        }

        var rnd = new Random();
        horde += rnd.Next(5, 8);
        alliance += rnd.Next(5, 8);

        return new FactionStatisticsModel
        {
            HordePlayers = horde,
            AlliancePlayers = alliance
        };
    }

    public async Task<LastBossKillModel?> GetLastBossKillAsync()
    {
        using var db = new MySqlConnection(_conn);
        await db.OpenAsync();

        const string sql = """
            SELECT ca.guid, c.name, ca.achievement, ca.date
            FROM character_achievement ca
            JOIN characters c ON ca.guid = c.guid
            ORDER BY ca.date DESC LIMIT 1;
            """;

        var row = await db.QueryFirstOrDefaultAsync(sql);
        if (row == null) return null;

        return new LastBossKillModel
        {
            CharacterName = row.name,
            AchievementName = $"Achievement ID: {row.achievement}",
            Date = row.date
        };
    }

    private static bool IsHorde(int race) => race is 2 or 5 or 6 or 8 or 10;
    private static bool IsAlliance(int race) => race is 1 or 3 or 4 or 7 or 11;
}
