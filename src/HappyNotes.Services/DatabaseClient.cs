using HappyNotes.Services.interfaces;
using Microsoft.Extensions.Configuration;
using SqlSugar;

namespace HappyNotes.Services;

public class DatabaseClient : IDatabaseClient
{
    private readonly SqlSugarClient _client;

    public DatabaseClient(IConfiguration configuration)
    {
        var connectionString = configuration.GetSection("ManticoreConnectionOptions:ConnectionString").Value;
        _client = new SqlSugarClient(new ConnectionConfig
        {
            ConnectionString = connectionString,
            DbType = DbType.MySql,
            IsAutoCloseConnection = false
        });
    }

    public async Task<List<T>> SqlQueryAsync<T>(string sql, object parameters)
    {
        return await _client.Ado.SqlQueryAsync<T>(sql, parameters);
    }

    public async Task<int> GetIntAsync(string sql, object parameters)
    {
        return await _client.Ado.GetIntAsync(sql, parameters);
    }

    public async Task<int> ExecuteCommandAsync(string sql, object parameters)
    {
        return await _client.Ado.ExecuteCommandAsync(sql, parameters);
    }
}
