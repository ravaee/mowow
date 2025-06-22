using AzerothCoreIntegration.Models;
using Dapper;
using MySql.Data.MySqlClient;

namespace AzerothCoreIntegration.Services
{
    public class CharacterService
    {
        private readonly string _charactersDb;
        private readonly string _authDb;

        public CharacterService(IConfiguration config)
        {
            _charactersDb = config.GetConnectionString("AzerothCoreCharactersDatabase");
            _authDb = config.GetConnectionString("AzerothCoreAuthDatabase");
        }

        public async Task<int?> GetAccountIdAsync(string gameAccountName)
        {
            using var conn = new MySqlConnection(_authDb);
            return await conn.QueryFirstOrDefaultAsync<int?>("SELECT id FROM account WHERE username = @username", new { username = gameAccountName });
        }

        public async Task<List<CharacterModel>> GetCharactersAsync(int accountId)
        {
            using var conn = new MySqlConnection(_charactersDb);
            var sql = "SELECT guid, name, level, money, xp, race, class, gender FROM characters WHERE account = @accountId";
            return (await conn.QueryAsync<CharacterModel>(sql, new { accountId })).ToList();
        }

        public async Task<List<AchievementModel>> GetAchievementsAsync(ulong guid)
        {
            using var conn = new MySqlConnection(_charactersDb);
            var sql = "SELECT achievement AS AchievementId, date FROM character_achievement WHERE guid = @guid ORDER BY date DESC";
            return (await conn.QueryAsync<AchievementModel>(sql, new { guid })).ToList();
        }

        public async Task<List<ReputationModel>> GetReputationsAsync(ulong guid)
        {
            using var conn = new MySqlConnection(_charactersDb);
            var sql = "SELECT faction, standing FROM character_reputation WHERE guid = @guid";
            return (await conn.QueryAsync<ReputationModel>(sql, new { guid })).ToList();
        }
    }
}
