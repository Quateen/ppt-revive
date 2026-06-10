namespace PPTRevive.Application.Common.Models;
public class DoctorDto
{
    public int Id { get; set; }
    public string? EmployeeNo { get; set; }
    public int UserId { get; set; }
    public string? Name { get; set; }
    public string? MobileNo { get; set; }
    public int City { get; set; }
    public string? CityName { get; set; }
    public string? Email { get; set; }
    public int Gender { get; set; }
    public string? GenderName { get; set; }
    public int Status { get; set; }
    public string? StatusName { get; set; }
    public DateTimeOffset Created { get; set; }
    public  int TotalCount { get; set; }
    public string? Type { get; set; }
}
