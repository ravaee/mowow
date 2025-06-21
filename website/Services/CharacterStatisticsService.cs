using MySql.Data.MySqlClient;
using Dapper;
using AzerothCoreIntegration.Models;

namespace AzerothCoreIntegration.Services;
public class CharacterStatisticsService(IConfiguration config)
{
    private readonly string _connectionString =
    config.GetConnectionString("AzerothCoreCharactersDatabase");

    public async Task<FactionStatisticsModel> GetFactionStatisticsAsync()
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        var sql = @"
            SELECT race, COUNT(*) AS count
            FROM characters
            WHERE online = 1
            GROUP BY race;";

        var result = await connection.QueryAsync<dynamic>(sql);

        int horde = 0;
        int alliance = 0;

        foreach (var row in result)
        {
            int race = row.race;
            int count = Convert.ToInt32(row.count); // Safely cast to int

            if (IsHorde(race))
                horde += count;
            else if (IsAlliance(race))
                alliance += count;
        }

        return new FactionStatisticsModel
        {
            HordePlayers = horde,
            AlliancePlayers = alliance
        };
    }

    public async Task<LastBossKillModel?> GetLastBossKillAsync()
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        var sql = @"
            SELECT ca.guid, c.name, ca.achievement, ca.date
            FROM character_achievement ca
            JOIN characters c ON ca.guid = c.guid
            ORDER BY ca.date DESC
            LIMIT 1;";

        var row = await connection.QueryFirstOrDefaultAsync(sql);
        if (row == null) return null;

        return new LastBossKillModel
        {
            CharacterName = row.name,
            AchievementName = $"Achievement ID: {row.achievement}",
            Date = row.date
        };
    }

    private bool IsHorde(int race) => race switch
    {
        2 or 5 or 6 or 8 or 10 => true,
        _ => false
    };

    private bool IsAlliance(int race) => race switch
    {
        1 or 3 or 4 or 7 or 11 => true,
        _ => false
    };
}


