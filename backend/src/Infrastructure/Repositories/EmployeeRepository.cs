using PPTRevive.Application.Common.Interfaces;
using PPTRevive.Application.Common.Models;
using Dapper;

namespace PPTRevive.Infrastructure.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly IDbRepository _dbRepository;

    public EmployeeRepository(IDbRepository dbRepository)
    {
        _dbRepository = dbRepository;
    }

    public async Task<IEnumerable<EmployeeDto>> GetEmployees(int employeeId, int pageNo, int pageSize)
    {
        var dynamicParameters = new DynamicParameters();
        dynamicParameters.Add("@p_employee_id", pageNo);
        dynamicParameters.Add("@p_page_no", pageNo);
        dynamicParameters.Add("@p_page_size", pageSize);

        var employees = await _dbRepository.QueryAsync<EmployeeDto>("employee_get_all_paginated", dynamicParameters, System.Data.CommandType.StoredProcedure);
        return employees;
    }

    public async Task<Tuple<SalaryDto?, IEnumerable<SalaryEmployeeDto>>> GetSalaryById(int salaryId)
    {
        var dynamicParameters = new DynamicParameters();
        dynamicParameters.Add("@p_salary_id", salaryId);

        var result = await _dbRepository.QueryAsync<SalaryDto, SalaryEmployeeDto>("salary_get_by_id", dynamicParameters, System.Data.CommandType.StoredProcedure);
        return result;
    }

    public async Task<Doctor?> DoctorReport(int DoctorId)
    {
        var dynamicParameters = new DynamicParameters();
        dynamicParameters.Add("@p_doctor_id", DoctorId);
        return await _dbRepository.QuerySingleOrDefaultAsync<Doctor?>("patients_count_by_doctor", dynamicParameters, System.Data.CommandType.StoredProcedure);
    }

    public async Task<Patient?> PatientReport()
    {
        var dynamicParameters = new DynamicParameters();
        return await _dbRepository.QuerySingleOrDefaultAsync<Patient>("patient_report", dynamicParameters, System.Data.CommandType.StoredProcedure);
    }
    public async Task<IEnumerable<PatientDto>> Patientlist()
    {
        var dynamicParameters = new DynamicParameters();
        return await _dbRepository.QueryAsync<PatientDto>("patient_list_today", dynamicParameters, System.Data.CommandType.StoredProcedure);
    }
}
