using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildingBlocks.Persistence.Configurations;

public class AttachmentConfiguration : AggregateRootConfiguration<Attachment>
{
    public override void Configure(EntityTypeBuilder<Attachment> builder)
    {
        builder.ToTable("Attachments", t => t.ExcludeFromMigrations());
        builder.ConfigureForBase();
    }
}