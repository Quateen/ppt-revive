using System.Data;

namespace PPTRevive.Application.Common.Interfaces;

public interface IDbRepository
{
    int Execute(string query, object param);
    int Execute(string query, object param, CommandType commandType);
    int Execute(string query, CommandType commandType);
    Task<int> ExecuteAsync(string query, object param, CommandType commandType);
    T? QuerySingleOrDefault<T>(string query, object param);
    T? QuerySingleOrDefault<T>(string query, CommandType commandType);
    T? QuerySingleOrDefault<T>(string query, object param, CommandType commandType);
    IEnumerable<T> Query<T>(string query, object param);
    IEnumerable<T> Query<T>(string query, CommandType commandType);
    IEnumerable<T> Query<T>(string query, object param, CommandType commandType);
    Task<T?> QuerySingleOrDefaultAsync<T>(string query, object param, CommandType commandType);
    Task<IEnumerable<T>> QueryAsync<T>(string query, object param, CommandType commandType);
    Task<IEnumerable<T>> QueryAsync<T>(string query, object param);
    Task<T?> ExecuteScalarAsync<T>(string query, object param, CommandType commandType);
    Task<Tuple<T1?, IEnumerable<T2>>> QueryAsync<T1, T2>(string query, object param, CommandType commandType);
    Task<Tuple<IEnumerable<T1>, IEnumerable<T2>>> QueryMultipleAsync<T1, T2>(string query, object param, CommandType commandType);
    Task<Tuple<IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>>> QueryMultipleAsync<T1, T2, T3>(string query, object param, CommandType commandType);
    Task<Tuple<IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>>> QueryMultipleAsync<T1, T2, T3, T4>(string query, object param, CommandType commandType);
}
