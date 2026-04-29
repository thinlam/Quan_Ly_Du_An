using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities;

namespace QLDA.Persistence.Configurations;

public class CauHinhVaiTroQuyenConfiguration : AggregateRootConfiguration<CauHinhVaiTroQuyen> {
    private static readonly DateTimeOffset SeedCreatedAt = new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public override void Configure(EntityTypeBuilder<CauHinhVaiTroQuyen> builder) {
        builder.ToTable("CauHinhVaiTroQuyen");
        builder.ConfigureForBase();

        builder.Property(e => e.VaiTro)
            .HasColumnOrder(5)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.QuyenId)
            .HasColumnOrder(6);

        builder.Property(e => e.KichHoat)
            .HasColumnOrder(7)
            .HasDefaultValue(true);

        builder.HasIndex(e => new { e.VaiTro, e.QuyenId })
            .IsUnique();

        SeedRolePermissions(builder);
    }

    private static void SeedRolePermissions(EntityTypeBuilder<CauHinhVaiTroQuyen> builder) {
        var id = 1;
        var data = new List<CauHinhVaiTroQuyen>();

        // QLDA_TatCa → all permissions
        foreach (var perm in GetAllPermissionMAs()) {
            data.Add(MakeEntry(ref id, RoleConstants.QLDA_TatCa, perm));
        }

        // QLDA_QuanTri → all permissions
        foreach (var perm in GetAllPermissionMAs()) {
            data.Add(MakeEntry(ref id, RoleConstants.QLDA_QuanTri, perm));
        }

        // QLDA_LDDV → XemTatCa only
        foreach (var perm in PermissionConstants.AllXemTatCa) {
            data.Add(MakeEntry(ref id, RoleConstants.QLDA_LDDV, perm));
        }

        // QLDA_LD → XemTatCa only
        foreach (var perm in PermissionConstants.AllXemTatCa) {
            data.Add(MakeEntry(ref id, RoleConstants.QLDA_LD, perm));
        }

        // QLDA_ChuyenVien → XemTheoPhong + Tao/Sua for own department
        foreach (var perm in PermissionConstants.AllXemTheoPhong) {
            data.Add(MakeEntry(ref id, RoleConstants.QLDA_ChuyenVien, perm));
        }
        foreach (var permissions in PermissionConstants.ByNhom.Values) {
            foreach (var perm in permissions.Where(p => p.EndsWith(".Tao") || p.EndsWith(".Sua"))) {
                data.Add(MakeEntry(ref id, RoleConstants.QLDA_ChuyenVien, perm));
            }
        }

        builder.HasData(data);
    }

    private static CauHinhVaiTroQuyen MakeEntry(ref int id, string vaiTro, string quyenMa) {
        return new CauHinhVaiTroQuyen {
            Id = id++,
            VaiTro = vaiTro,
            QuyenId = GetQuyenId(quyenMa),
            KichHoat = true,
            CreatedAt = SeedCreatedAt,
        };
    }

    /// <summary>
    /// Maps permission Ma to DanhMucQuyen seed Id (must match DanhMucQuyenConfiguration seed order)
    /// </summary>
    private static int GetQuyenId(string ma) {
        var id = 1;
        foreach (var (_, permissions) in PermissionConstants.ByNhom) {
            foreach (var perm in permissions) {
                if (perm == ma) return id;
                id++;
            }
        }
        return id;
    }

    private static IEnumerable<string> GetAllPermissionMAs() {
        return PermissionConstants.ByNhom.Values.SelectMany(p => p);
    }
}
