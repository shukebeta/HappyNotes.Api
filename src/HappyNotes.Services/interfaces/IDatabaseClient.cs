using System.Collections.Generic;
using System.Threading.Tasks;

namespace HappyNotes.Services.interfaces;

public interface IDatabaseClient
{
    Task<List<T>> SqlQueryAsync<T>(string sql, object parameters);
    Task<int> GetIntAsync(string sql, object parameters);
    Task<int> ExecuteCommandAsync(string sql, object parameters);
}