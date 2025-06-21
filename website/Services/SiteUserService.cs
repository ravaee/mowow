using MySql.Data.MySqlClient;
using Dapper;
using AzerothCoreIntegration.Models;
using System.Security.Cryptography;

public class SiteUserService
{
    private readonly string _connectionString;

    public SiteUserService(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("AzerothCoreAuthDatabase");
    }

    public async Task EnsureTableExistsAsync()
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        var sql = @"
            CREATE TABLE IF NOT EXISTS siteUsers (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                Email VARCHAR(256) NOT NULL UNIQUE,
                GameAccountName VARCHAR(17) NOT NULL UNIQUE,
                PasswordHash VARCHAR(512) NOT NULL,
                CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
            );
        ";

        await connection.ExecuteAsync(sql);
    }

    public async Task<bool> IsEmailUsedAsync(string email)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        var sql = "SELECT COUNT(*) FROM siteUsers WHERE Email = @Email";
        var count = await connection.ExecuteScalarAsync<long>(sql, new { Email = email });
        return count > 0;
    }

    public async Task InsertSiteUserAsync(string email, string gameAccountName, string password)
    {
        var hash = HashPassword(password);

        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        var sql = "INSERT INTO siteUsers (Email, GameAccountName, PasswordHash) VALUES (@Email, @GameAccountName, @PasswordHash)";
        await connection.ExecuteAsync(sql, new { Email = email, GameAccountName = gameAccountName, PasswordHash = hash });
    }

    public async Task<SiteUser?> GetByEmailAsync(string email)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        var sql = "SELECT * FROM siteUsers WHERE Email = @Email";
        return await connection.QueryFirstOrDefaultAsync<SiteUser>(sql, new { Email = email });
    }

    public bool VerifyPassword(string password, string storedHash)
    {
        var parts = storedHash.Split(':');
        var salt = Convert.FromBase64String(parts[0]);
        var stored = parts[1];

        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);
        var hashBytes = pbkdf2.GetBytes(32);
        var hashString = Convert.ToBase64String(hashBytes);

        return hashString == stored;
    }

    private string HashPassword(string password)
    {
        using var rng = RandomNumberGenerator.Create();
        byte[] salt = new byte[16];
        rng.GetBytes(salt);

        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);
        var hash = pbkdf2.GetBytes(32);
        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }
}
