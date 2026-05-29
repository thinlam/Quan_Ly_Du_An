using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QLDA.Persistence.Configurations;

public class DonViTuVanKeHoachConfiguration : AggregateRootConfiguration<DonViTuVanKeHoach> {
    public override void Configure(EntityTypeBuilder<DonViTuVanKeHoach> builder) {

        builder.ToTable(nameof(DonViTuVanKeHoach));

        // 1. Chỉ định Id làm Khóa chính duy nhất
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
               .HasColumnName("Id")
               .HasDefaultValueSql("newid()"); // Tự động tạo GUID mới khi Insert nếu không truyền

        // 3. SỬA: Đổi tên cột KeHoachId thành KeHoachLCNTId trong DB (Cột này là Khóa Ngoại, không phải Khóa Chính)
        builder.Property(e => e.KeHoachId)
               .HasColumnName("KeHoachLCNTId")
               .IsRequired();

        // 4. Cấu hình độ dài chuỗi cho Tên đơn vị
        builder.Property(e => e.TenDonVi)
               .HasColumnType("nvarchar(500)")
               .IsRequired(true); // Cho phép Null nếu cần, hoặc .IsRequired() nếu bắt buộc

        // 5. Gợi ý thêm: Cấu hình luôn mối quan hệ (Foreign Key) nếu cần thiết
        builder.HasOne(e => e.TrienKhaiKeHoach)
               .WithMany(k => k.DonViTuVans)
               .HasForeignKey(e => e.KeHoachId)
               .OnDelete(DeleteBehavior.Cascade);



    }
}