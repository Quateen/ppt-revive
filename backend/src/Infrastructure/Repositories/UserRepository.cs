using PPTRevive.Application.Common.Interfaces;
using PPTRevive.Application.Common.Models;
using Dapper;

namespace PPTRevive.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IDbRepository _dbRepository;

    public UserRepository(IDbRepository dbRepository)
    {
        _dbRepository = dbRepository;
    }

    public async Task<IEnumerable<UserDto>> GetUsers(int pageNo, int pageSize)
    {
        var dynamicParameters = new DynamicParameters();
        //dynamicParameters.Add("@p_user_id", pageNo);
        dynamicParameters.Add("@p_page_no", pageNo);
        dynamicParameters.Add("@p_page_size", pageSize);

        var users = await _dbRepository.QueryAsync<UserDto>("user_get_all_paginated", dynamicParameters, System.Data.CommandType.StoredProcedure);
        return users;
    }
    public async Task<IEnumerable<DoctorDto>> GetAllEmployees(int pageNumber, int pageSize, string? name, DateTime? startDate, DateTime? endDate)
    {
        var dynamicParameters = new DynamicParameters();
        dynamicParameters.Add("@p_page_no", pageNumber);
        dynamicParameters.Add("@p_page_size", pageSize);
        dynamicParameters.Add("@p_name", name);
        dynamicParameters.Add("@p_start_date", startDate);
        dynamicParameters.Add("@p_end_date", endDate);

        var employees = await _dbRepository.QueryAsync<DoctorDto>("employee_listing", dynamicParameters, System.Data.CommandType.StoredProcedure);
        return employees;
    }

}
