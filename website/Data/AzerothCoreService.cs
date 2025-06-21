using System.Security.Cryptography;
using System.Text;
using Dapper;
using MySql.Data.MySqlClient;

namespace WowWebsite.Data;

public class AzerothCoreService(IConfiguration cfg)
{
    private readonly string _conn = cfg.GetConnectionString("MySql");

    /// <summary>
    /// Creates both a WoW account and a website user atomically in a single MySQL transaction.
    /// </summary>
    public async Task CreateAccountAsync(string username, string password, string email)
    {
        var shaHash = GenerateShaPassHash(username, password);
        await using var conn = new MySqlConnection(_conn);
        await conn.OpenAsync();
        await using var tx = await conn.BeginTransactionAsync();

        // 1) insert into AzerothCore account table
        var sql1 = "INSERT INTO account (username, sha_pass_hash, email) VALUES (@u,@h,@e)";
        await conn.ExecuteAsync(sql1, new { u = username, h = shaHash, e = email }, tx);

        // 2) insert into website_users (new table in same DB)
        var sql2 = @"CREATE TABLE IF NOT EXISTS website_users (
                        id INT AUTO_INCREMENT PRIMARY KEY,
                        username VARCHAR(50) UNIQUE NOT NULL,
                        sha_pass_hash VARCHAR(40) NOT NULL,
                        email VARCHAR(100)
                      ) ENGINE=InnoDB;";
        await conn.ExecuteAsync(sql2, transaction: tx);

        var sql3 = "INSERT INTO website_users (username, sha_pass_hash, email) VALUES (@u,@h,@e)";
        await conn.ExecuteAsync(sql3, new { u = username, h = shaHash, e = email }, tx);

        await tx.CommitAsync();
    }

    public string GenerateShaPassHash(string username, string password)
    {
        var raw = $"{username.ToUpper()}:{password.ToUpper()}";
        using var sha1 = SHA1.Create();
        var bytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(raw));
        return BitConverter.ToString(bytes).Replace("-", "");
    }
}
