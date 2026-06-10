using PPTRevive.Application.Common.Models;
namespace PPTRevive.Application.Common.Interfaces;

public interface IUserRepository
{
    Task<IEnumerable<UserDto>> GetUsers(int pageNo, int pageSize);
    Task<IEnumerable<DoctorDto>> GetAllEmployees(int pageNumber, int pageSize, string? name, DateTime? startDate, DateTime? endDate);
}
