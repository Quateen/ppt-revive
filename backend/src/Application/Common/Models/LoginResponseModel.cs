namespace PPTRevive.Application.Common.Models;

public class UserLoginModel
{
    public long Id { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Token { get; set; }
    public string? RoleName { get; set; }
    public List<RightsModel>? Rights { get; set; }
}

public class RightsModel
{
    public int RightsId { get; set; }
    public string? RightsName { get; set; }
}
