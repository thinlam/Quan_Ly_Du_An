namespace QLDA.Application.DuAnBuocs.DTOs {
    public class DuAnBuocDuAnUpdateStateDto : IHasKey<int> {
        public int Id { get; set; }
        public int? TrangThaiId { get; set; }
        public DateOnly? NgayDuKienBatDau { get; set; }
        public DateOnly? NgayDuKienKetThuc { get; set; }
        public DateOnly? NgayThucTeBatDau { get; set; }
        public DateOnly? NgayThucTeKetThuc { get; set; }
        [DefaultValue(null)] public string? GhiChu { get; set; }
        [DefaultValue(null)] public string? TrachNhiemThucHien { get; set; }
        [DefaultValue(false)] public bool IsKetThuc { get; set; }
        /// <summary>
        /// Phòng ban phụ trách chính - FK to DanhMucDonVi (legacy table)
        /// </summary>
        public long? PhongPhuTrachChinhId { get; set; }
        /// <summary>
        /// Danh sách phòng ban phối hợp - FK to DanhMucDonVi (legacy table).
        /// Tất cả ID phải thuộc DuAn.DuAnChiuTrachNhiemXuLys (Loai=DonViPhoiHop).
        /// </summary>
        public List<long>? DanhSachPhongBanPhoiHopIds { get; set; } = [];
    }
}
