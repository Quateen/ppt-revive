//using System.ComponentModel.DataAnnotations.Schema;
//using PPTRevive.Domain.Entities;

//namespace PPTRevive.Application.Salarys.Queries.GetSalariesWithPagination;

//public class RoleRightDto
//{
//    public int Id { get; set; }
//    public int RoleId { get; set; }
//    public Role? Role { get; set; }
//    public int RightId { get; set; }
//    public string? RightName { get; set; }
//    public bool CanView { get; set; }
//    public bool CanAdd { get; set; }
//    public bool CanUpdate { get; set; }
//    public bool CanDelete { get; set; }

//    private class Mapping : Profile
//    {
//        public Mapping()
//        {
//            CreateMap<RoleRight, RoleRightDto>()
//                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role));
//                //.ForMember(dest => dest.Right, opt => opt.MapFrom(src => src.Right));
//        }
//    }
//}
