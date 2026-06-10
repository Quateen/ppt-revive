//using PPTRevive.Domain.Entities;

//namespace PPTRevive.Application.Roles.Queries.GetRolesWithPagination;

//public class RoleDto
//{
//    public int Id { get; set; }
//    public string? Name { get; set; }
//    public string? Description { get; set; }
//    public List<RightDto> Rights { get; set; } = new List<RightDto>();

//    private class Mapping : Profile
//    {
//        public Mapping()
//        {
//            CreateMap<Role, RoleDto>()
//                .ForMember(dest => dest.Rights, opt => opt.MapFrom(src => src.RoleRights.Select(rr => rr.Right)));
//        }
//    }
//}

//public class RightDto
//{
//    public int Id { get; set; }
//    public string? Name { get; set; }
//    public string? Description { get; set; }
//}
