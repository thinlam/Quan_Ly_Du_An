using BuildingBlocks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QLDA.Persistence.Configurations;

public class AttachmentConfiguration : BuildingBlocks.Persistence.Configurations.AttachmentConfiguration {
    public override void Configure(EntityTypeBuilder<Attachment> builder) {
        builder.ToTable("TepDinhKem");
        builder.ConfigureForBase();
    }
}
