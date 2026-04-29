using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Persistence.Configurations.DanhMuc;

public class DanhMucQuyenConfiguration : AggregateRootConfiguration<DanhMucQuyen> {
    private static readonly DateTimeOffset SeedCreatedAt = new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public override void Configure(EntityTypeBuilder<DanhMucQuyen> builder) {
        builder.ToTable("DmQuyen");
        builder.ConfigureForDanhMuc();

        builder.Property(e => e.NhomQuyen)
            .HasColumnOrder(5)
            .HasMaxLength(50);

        builder.HasIndex(e => new { e.Ma, e.NhomQuyen })
            .IsUnique()
            .HasFilter("[Ma] IS NOT NULL AND [Ma] <> ''");

        builder.HasMany(e => e.CauHinhVaiTroQuyens)
            .WithOne(e => e.Quyen)
            .HasForeignKey(e => e.QuyenId)
            .OnDelete(DeleteBehavior.Restrict);

        SeedPermissions(builder);
    }

    private static void SeedPermissions(EntityTypeBuilder<DanhMucQuyen> builder) {
        var id = 1;
        var data = new List<DanhMucQuyen>();

        foreach (var (nhom, permissions) in PermissionConstants.ByNhom) {
            for (var i = 0; i < permissions.Length; i++) {
                var ma = permissions[i];
                var action = ma.Split('.')[1];
                data.Add(new DanhMucQuyen {
                    Id = id++,
                    Ma = ma,
                    Ten = $"{MapActionName(action)} {MapGroupName(nhom)}",
                    NhomQuyen = nhom,
                    Stt = i + 1,
                    Used = true,
                    CreatedAt = SeedCreatedAt,
                });
            }
        }

        builder.HasData(data);
    }

    private static string MapActionName(string action) => action switch {
        "XemTatCa" => "Xem tất cả",
        "XemTheoPhong" => "Xem theo phòng",
        "Tao" => "Tạo",
        "Sua" => "Sửa",
        "Xoa" => "Xóa",
        "PheDuyet" => "Phê duyệt",
        "QuanLy" => "Quản lý",
        _ => action
    };

    private static string MapGroupName(string nhom) => nhom switch {
        "DuAn" => "dự án",
        "GoiThau" => "gói thầu",
        "HopDong" => "hợp đồng",
        "VanBan" => "văn bản",
        "ThanhToan" => "thanh toán",
        _ => nhom
    };
}
