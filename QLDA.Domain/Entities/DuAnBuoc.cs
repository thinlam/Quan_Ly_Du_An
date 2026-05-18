using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Domain.Entities;

public class DuAnBuoc : Entity<int>, IAggregateRoot, IHasUsed {
    public int BuocId { get; set; }
    public Guid DuAnId { get; set; }
    public string? TenBuoc { get; set; }
    public string? PartialView { get; set; }
    public bool Used { get; set; }
    public int? TrangThaiId { get; set; }
    public DateTimeOffset? NgayDuKienBatDau { get; set; }
    public DateTimeOffset? NgayDuKienKetThuc { get; set; }
    public DateTimeOffset? NgayThucTeBatDau { get; set; }
    public DateTimeOffset? NgayThucTeKetThuc { get; set; }
    public string? GhiChu { get; set; }
    public string? TrachNhiemThucHien { get; set; }
    /// <summary>
    /// Trạng thái kết thúc của bước
    /// </summary>
    /// <remarks>
    /// Bắt buộc check isKetThuc thì phải có ngày kết thúc thực tế
    /// </remarks>
    public bool IsKetThuc { get; set; }

    /// <summary>
    /// Phòng phụ trách chính - FK to DmDonVi (legacy table, no navigation property)
    /// </summary>
    public long? PhongPhuTrachChinhId { get; set; }

    #region Navigation Properties

    public DuAn? DuAn { get; set; }
    public DuAn? DuAnHienTai { get; set; }
    public DanhMucBuoc? Buoc { get; set; }
    public ICollection<DuAnBuocManHinh>? DuAnBuocManHinhs { get; set; } = [];
    public ICollection<BaoCao>? BaoCaos { get; set; } = [];
    public ICollection<VanBanQuyetDinh>? VanBanQuyetDinhs { get; set; } = [];
    public ICollection<DangTaiKeHoachLcntLenMang>? DangTaiKeHoachLcntLenMangs { get; set; } = [];
    public ICollection<KetQuaTrungThau>? KetQuaTrungThaus { get; set; } = [];
    public ICollection<HopDong>? HopDongs { get; set; } = [];
    public ICollection<PhuLucHopDong>? PhuLucHopDongs { get; set; } = [];
    public ICollection<TamUng>? TamUngs { get; set; } = [];
    public ICollection<ThanhToan>? ThanhToans { get; set; } = [];
    public ICollection<DuAnBuocPhongBanPhoiHop>? DuAnBuocPhongBanPhoiHops { get; set; }

    #endregion
}