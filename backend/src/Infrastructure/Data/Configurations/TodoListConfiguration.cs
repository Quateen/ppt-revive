//using PPTRevive.Domain.Entities;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Metadata.Builders;

//namespace PPTRevive.Infrastructure.Data.Configurations;

//public class TodoListConfiguration : IEntityTypeConfiguration<TodoList>
//{
//    public void Configure(EntityTypeBuilder<TodoList> builder)
//    {
//        builder.Property(t => t.Title)
//            .HasMaxLength(200)
//            .IsRequired();

//        builder
//            .OwnsOne(b => b.Colour);
//    }
//}
