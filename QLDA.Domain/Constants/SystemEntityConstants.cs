namespace QLDA.Domain.Constants;

/// <summary>
/// Entity names for export functionality
/// </summary>
public static class SystemEntityConstants {
    public const string DuAn = "du-an";
    public const string GoiThau = "goi-thau";
    public const string HopDong = "hop-dong";
    public const string PhuLucHopDong = "phu-luc-hop-dong";
    public const string BaoCaoTienDo = "bao-cao-tien-do";
    public const string BaoCaoBaoHanhSanPham = "bao-cao-bao-hanh-san-pham";
    public const string BaoCaoBanGiaoSanPham = "bao-cao-ban-giao-san-pham";
    public const string KhoKhanVuongMac = "kho-khan-vuong-mac";
    public const string TongHopVanBanQuyetDinh = "tong-hop-van-ban-quyet-dinh";

    public static readonly Dictionary<string, string> All = new() {
        [DuAn] = "Dự án",
        [GoiThau] = "Gói thầu",
        [HopDong] = "Hợp đồng",
        [PhuLucHopDong] = "Phụ lục hợp đồng",
        [BaoCaoTienDo] = "Báo cáo tiến độ",
        [BaoCaoBaoHanhSanPham] = "Báo cáo bảo hành sản phẩm",
        [BaoCaoBanGiaoSanPham] = "Báo cáo bàn giao sản phẩm",
        [KhoKhanVuongMac] = "Khó khăn vướng mắc",
        [TongHopVanBanQuyetDinh] = "Tổng hợp văn bản quyết định"
    };
}