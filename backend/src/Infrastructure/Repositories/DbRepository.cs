using System.Data;
using System.Data.Common;
using PPTRevive.Application.Common.Interfaces;
using Dapper;

namespace PPTRevive.Infrastructure.Repositories;

public class DbRepository : IDbRepository
{
    private readonly DbConnection _connection;

    public DbRepository(DbConnection connection)
    {
        _connection = connection;
    }

    public int Execute(string query, object param, CommandType commandType)
    {
        using (_connection)
        {
            _connection.Open();
            var result = _connection.Execute(query, param, null, null, commandType);
            return result;
        }

    }

    public Task<int> ExecuteAsync(string query, object param, CommandType commandType)
    {
        using (_connection)
        {
            _connection.Open();
            var result = _connection.ExecuteAsync(query, param, null, null, commandType);
            return result;
        }
    }

    public T? QuerySingleOrDefault<T>(string query, object param)
    {
        using (_connection)
        {
            _connection.Open();
            return _connection.QuerySingleOrDefault<T>(query, param);
        }
    }

    public T? QuerySingleOrDefault<T>(string query, object param, CommandType commandType)
    {
        using (_connection)
        {
            _connection.Open();
            return _connection.QuerySingleOrDefault<T>(query, param, null, null, commandType);
        }

    }

    public T? QuerySingleOrDefault<T>(string query, CommandType commandType)
    {
        using (_connection)
        {
            _connection.Open();
            var result = _connection.QuerySingleOrDefault<T>(query, null, null, null, commandType);
            return result;
        }

    }
    public IEnumerable<T> Query<T>(string query, object param)
    {
        using (_connection)
        {
            _connection.Open();
            var result = _connection.Query<T>(query, param);
            return result;
        }
    }
    public IEnumerable<T> Query<T>(string query, object param, CommandType commandType)
    {
        using (_connection)
        {
            _connection.Open();
            var result = _connection.Query<T>(query, param, null, true, null, commandType);
            return result;
        }
    }
    public IEnumerable<T> Query<T>(string query, CommandType commandType)
    {
        using (_connection)
        {
            _connection.Open();
            var result = _connection.Query<T>(query, null, null, true, null, commandType);
            return result;
        }
    }

    public int Execute(string query, CommandType commandType)
    {
        using (_connection)
        {
            _connection.Open();
            var result = _connection.Execute(query, null, null, null, commandType);
            return result;
        }
    }

    public int Execute(string query, object param)
    {
        using (_connection)
        {
            _connection.Open();
            var result = _connection.Execute(query, param);
            return result;
        }
    }

    public async Task<T?> QuerySingleOrDefaultAsync<T>(string query, object param, CommandType commandType)
    {
        using (_connection)
        {
            _connection.Open();
            var result = await _connection.QuerySingleOrDefaultAsync<T>(query, param, null, null, commandType);
            return result;
        }
    }

    public async Task<IEnumerable<T>> QueryAsync<T>(string query, object param, CommandType commandType)
    {
        using (_connection)
        {
            _connection.Open();
            var result = await _connection.QueryAsync<T>(query, param, null, null, commandType);
            return result;
        }
    }

    public async Task<IEnumerable<T>> QueryAsync<T>(string query, object param)
    {
        using (_connection)
        {
            _connection.Open();
            var result = await _connection.QueryAsync<T>(query, param);
            return result;
        }
    }

    public async Task<T?> ExecuteScalarAsync<T>(string query, object param, CommandType commandType)
    {
        using (_connection)
        {
            _connection.Open();
            var result = await _connection.ExecuteScalarAsync<T>(query, param, null, null, commandType);
            return result;
        }
    }

    public async Task<Tuple<T1?, IEnumerable<T2>>> QueryAsync<T1, T2>(string query, object param, CommandType commandType)
    {
        try
        {
            using (_connection)
            {
                _connection.Open();
                var result = await _connection.QueryMultipleAsync(query, param, null, null, commandType);

                var list1 = result.Read<T1>().ToList();
                var res1 = list1.Count > 0 ? list1.First() : default;
                var res2 = result.Read<T2>();

                return new Tuple<T1?, IEnumerable<T2>>(res1, res2 ?? Enumerable.Empty<T2>());
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<Tuple<IEnumerable<T1>, IEnumerable<T2>>> QueryMultipleAsync<T1, T2>(string query, object param, CommandType commandType)
    {
        try
        {
            using (_connection)
            {
                _connection.Open();
                var result = await _connection.QueryMultipleAsync(query, param, null, null, commandType);
                var res1 = result.Read<T1>();
                var res2 = result.Read<T2>();

                return new Tuple<IEnumerable<T1>, IEnumerable<T2>>(res1, res2);
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<Tuple<IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>>> QueryMultipleAsync<T1, T2, T3>(string query, object param, CommandType commandType)
    {
        using (_connection)
        {
            _connection.Open();
            var result = await _connection.QueryMultipleAsync(query, param, null, null, commandType);
            var res1 = result.Read<T1>();
            var res2 = result.Read<T2>();
            var res3 = result.Read<T3>();

            return new Tuple<IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>>(res1, res2, res3);
        }
    }

    public async Task<Tuple<IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>>> QueryMultipleAsync<T1, T2, T3, T4>(string query, object param, CommandType commandType)
    {
        using (_connection)
        {
            _connection.Open();
            var result = await _connection.QueryMultipleAsync(query, param, null, null, commandType);
            var res1 = result.Read<T1>();
            var res2 = result.Read<T2>();
            var res3 = result.Read<T3>();
            var res4 = result.Read<T4>();

            return new Tuple<IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>>(res1, res2, res3, res4);
        }
    }
}
