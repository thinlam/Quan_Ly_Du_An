using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Domain.Entities;

/// <summary>
/// Bảng dự án
/// </summary>
public class DuAn : MaterializedPathEntity<Guid>, IAggregateRoot {
    public new Guid? ParentId { get; set; }

    /// <summary>
    /// Tên dự án
    /// </summary>
    public string? TenDuAn { get; set; }

    /// <summary>
    /// Địa điểm <br/>
    /// </summary>
    /// <remarks>: Mặc định TT CĐS Tp HCM (hoặc các địa phương của 3 tỉnh/thành) </remarks>
    public string? DiaDiem { get; set; }

    /// <summary>
    /// Danh mục chủ đầu tư
    /// </summary>
    public int? ChuDauTuId { get; set; }

    /// <summary>
    /// Năm dự kiến khởi công, hoàn thành 
    /// </summary>
    public int? ThoiGianKhoiCong { get; set; }

    /// <summary>
    /// Năm khởi công, hoàn thành thực tế 
    /// </summary>
    public int? ThoiGianHoanThanh { get; set; }

    /// <summary>
    /// Mã dự án
    /// </summary>
    public string? MaDuAn { get; set; }

    /// <summary>
    /// Mã ngân sách
    /// </summary>
    public string? MaNganSach { get; set; }

    /// <summary>
    /// Là dự án trọng điểm/ ưu tiên
    /// </summary>
    public bool DuAnTrongDiem { get; set; }

    /// <summary>
    /// Danh mục lĩnh vực
    /// </summary>
    public int? LinhVucId { get; set; }

    /// <summary>
    /// Danh mục nhóm dự án
    /// </summary>
    public int? NhomDuAnId { get; set; }

    /// <summary>
    /// Năng lực thiết kế
    /// </summary>
    public string? NangLucThietKe { get; set; }

    /// <summary>
    /// Quy mô dự án
    /// </summary>
    public string? QuyMoDuAn { get; set; }

    /// <summary>
    /// Danh mục hình thức quản lý dự án
    /// </summary>
    /// <remarks>Tự thực hiện, thuê, ...
    /// </remarks>
    public int? HinhThucQuanLyDuAnId { get; set; }

    /// <summary>
    /// Danh mục hình thức đầu tư
    /// </summary>
    /// <remarks>: - Mua sắm; thuê dịch vụ CNTT sẵn có <br/>
    /// Dự án hoặc Kế hoạch thuê dịch vụ công nghệ thông tin theo yêu cầu riêng
    /// </remarks>
    public int? HinhThucDauTuId { get; set; }

    /// <summary>
    /// Danh mục loại dự án
    /// </summary>
    /// <remarks>: Danh mục loại dự án: CĐS, đề án 06, kế hoạch của Tp HCM,….
    /// </remarks>
    public int? LoaiDuAnId { get; set; }

    /// <summary>
    /// Thương mại điển tử <br/>
    /// </summary>
    /// <remarks>: TMĐT theo QĐ phê duyệt chủ trương</remarks>
    public long? TongMucDauTu { get; set; }

    #region Thông tin dự toán hiện tại - mới nhất

    /// <summary>
    /// Số dự toán
    /// </summary>
    public long SoDuToan { get; set; }

    /// <summary>
    /// Năm dự toán
    /// </summary>
    public int NamDuToan { get; set; }

    /// <summary>
    /// Số quyết định dự toán
    /// </summary>
    public string? SoQuyetDinhDuToan { get; set; }

    /// <summary>
    /// Ngày ký dự toán
    /// </summary>
    public DateTimeOffset? NgayKyDuToan { get; set; }
    /// <summary>
    /// Dự toán hiện tại
    /// </summary>
    public Guid? DuToanHienTaiId { get; set; }
    #endregion

    /// <summary>
    /// Danh mục quy trình
    /// </summary>
    public int? QuyTrinhId { get; set; }

    /// <summary>
    /// Danh mục các bước trong quy trình của dự án
    /// </summary>
    public int? BuocHienTaiId { get; set; }

    /// <summary>
    /// Danh mục các bước trong quy trình của dự án
    /// </summary>
    public int? GiaiDoanHienTaiId { get; set; }

    /// <summary>
    /// Trạng thái tiến độ công việc hiện tại của dự án 
    /// </summary>
    public int? TrangThaiHienTaiId { get; set; }

    /// <summary>Trạng thái hiện tại của dự án.
    /// * Đang thực hiện 
    /// * Đã phê duyệt đầu tư 
    /// * Đã hoàn thành 
    /// * Tạm dừng 
    /// </summary>
    public int? TrangThaiDuAnId { get; set; }

    /// <summary>
    /// Loại dự án theo năm tài chính
    /// <list type="bullet">
    ///   <item> Chuẩn bị đầu tư </item>
    ///   <item> Chuyển tiếp </item>
    ///   <item> Khởi công mới </item>
    ///   <item> Khối lượng tồn đọng </item>
    /// </list>
    /// </summary>
    public int? LoaiDuAnTheoNamId { get; set; }

    /// <summary>
    /// Ghi chú
    /// </summary>
    public string? GhiChu { get; set; }

    /// <summary>
    /// Ngày bắt đầu thực hiện dự án 
    /// </summary>
    public DateTimeOffset? NgayBatDau { get; set; }
    /// <summary>
    /// Lãnh đạo phụ trách chính
    /// </summary>
    public long? LanhDaoPhuTrachId { get; set; }
    /// <summary>
    /// Đơn vị phụ trách chính
    /// </summary>
    public long? DonViPhuTrachChinhId { get; set; }


    #region Navigation Properties

    /// <summary>
    /// Danh sách dự toán của dự án
    /// </summary>
    public ICollection<DuToan>? DuToans { get; set; } = [];
    
    /// <summary>
    /// Dự toán ban đầu
    /// </summary>
    
    public DuToan? DuToanBanDau { get; set; }   // ← Navigation
    /// <summary>
    /// Khái toán kinh phí
    /// </summary>
    public decimal? KhaiToanKinhPhi { get; set; }
    public long? DuToanBanDauId { get; set; }
    
    public long? SoDuToanBanDau { get; set; }      // ← Số dự toán (bigint)
    public decimal? SoTienDuToanBanDau { get; set; } // ← Số tiền dự toán (nếu cần)
    #endregion



    #region Navigation Properties

    #region Self Reference
    public DuToan? DuToanHienTai { get; set; }
    public DuAnBuoc? BuocHienTai { get; set; }
    public DanhMucGiaiDoan? GiaiDoanHienTai { get; set; }
    public DanhMucTrangThaiDuAn? TrangThaiDuAn { get; set; }
    public DanhMucChuDauTu? ChuDauTu { get; set; }
    public DanhMucHinhThucDauTu? HinhThucDauTu { get; set; }
    public DanhMucHinhThucQuanLy? HinhThucQuanLy { get; set; }
    public DanhMucLinhVuc? LinhVuc { get; set; }
    public DanhMucLoaiDuAn? LoaiDuAn { get; set; }
    public DanhMucLoaiDuAnTheoNam? LoaiDuAnTheoNam { get; set; }
    public DanhMucNhomDuAn? NhomDuAn { get; set; }
    public DanhMucQuyTrinh? QuyTrinh { get; set; }
    public DanhMucTrangThaiTienDo? TrangThaiTienDo { get; set; }
    public ICollection<DuAnBuoc>? DuAnBuocs { get; set; } = [];
    public ICollection<DuAnNguonVon>? DuAnNguonVons { get; set; } = [];
    public ICollection<DuAnChiuTrachNhiemXuLy>? DuAnChiuTrachNhiemXuLys { get; set; } = [];

    #endregion

    public ICollection<VanBanQuyetDinh>? VanBanQuyetDinhs { get; set; } = [];
    public ICollection<BaoCao>? BaoCaos { get; set; } = [];
    public ICollection<GoiThau>? GoiThaus { get; set; } = [];
    public ICollection<HopDong>? HopDongs { get; set; } = [];
    public ICollection<KetQuaTrungThau>? KetQuaTrungThaus { get; set; } = [];
    public ICollection<PhuLucHopDong>? PhuLucHopDongs { get; set; } = [];
    public ICollection<NghiemThu>? NghiemThus { get; set; } = [];
    public ICollection<ThanhToan>? ThanhToans { get; set; } = [];
    public ICollection<TamUng>? TamUngs { get; set; } = [];

    #endregion
}