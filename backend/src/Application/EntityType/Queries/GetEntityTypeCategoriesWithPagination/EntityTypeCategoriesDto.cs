//using System.ComponentModel.DataAnnotations;
//using PPTRevive.Domain.Entities;

//namespace PPTRevive.Application.EntityTypes.Queries.GetEntityTypesWithPagination;

//public class EntityTypeCategoriesDto
//{
//    public int Id { get; set; }
//    [StringLength(200)]
//    public string? Name { get; set; }
//    [StringLength(500)]
//    public string? Description { get; set; }
//    public int? DisplayOrder { get; set; }
//    [StringLength(100)]
//    public string? ColorCode { get; set; }
//    public int EntityTypeId { get; set; }
//    public int? ParentCategoryId { get; set; }
    
//    private class Mapping : Profile
//    {
//        public Mapping()
//        {
//            CreateMap<Category, EntityTypeCategoriesDto>();
//        }
//    }
//}
