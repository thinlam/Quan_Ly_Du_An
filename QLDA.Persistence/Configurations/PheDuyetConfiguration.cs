using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QLDA.Persistence.Configurations;

public class PheDuyetConfiguration : IEntityTypeConfiguration<PheDuyet>
{
    public void Configure(EntityTypeBuilder<PheDuyet> builder)
    {
        builder.ToTable(nameof(PheDuyet));
       // builder.Property(x => x.So).HasMaxLength(200);
        
    }
}
