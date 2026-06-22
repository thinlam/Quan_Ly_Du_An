using QLDA.Application.BanGiaoHoSos.DTOs;
using QLDA.Application.BanGiaoHoSos.Queries;
using QLDA.Application.DeXuatChuyenTieps.DTOs;
using QLDA.Application.DeXuatChuyenTieps.Queries;
using QLDA.Application.DeXuatNhuCauKinhPhis.DTOs;
using QLDA.Application.DeXuatNhuCauKinhPhis.Queries;
using QLDA.Application.DuAns.DTOs;
using QLDA.Application.DuAnBuocs.DTOs;
using QLDA.Application.DuAnBuocs.Queries;
using QLDA.Application.GoiThaus.DTOs;
using QLDA.Application.HopDongs.DTOs;
using QLDA.Application.PhanKhaiKinhPhis.DTOs;
using QLDA.Application.PhanKhaiKinhPhis.Queries;
using QLDA.Application.TongHopDeXuatChuTruongs.DTOs;
using QLDA.Application.TongHopDeXuatChuTruongs.Queries;
using QLDA.Domain.Constants;
using QLDA.Infrastructure.Offices;
using BuildingBlocks.CrossCutting.Offices;
using QLDA.WebApi.Models.BaoCaoBanGiaoSanPhams;
using QLDA.WebApi.Models.BaoCaoBaoHanhSanPhams;
using QLDA.WebApi.Models.BaoCaoTienDos;
using QLDA.WebApi.Models.DeXuatChuTruongChuyenTieps;
using QLDA.WebApi.Models.DeXuatNhuCauKinhPhis;
using QLDA.WebApi.Models.KhoKhanVuongMacs;
using QLDA.WebApi.Models.PhanKhaiKinhPhis;
using QLDA.WebApi.Models.PhuLucHopDongs;
using QLDA.WebApi.Models.TongHopDeXuatChuTruongs;
using QLDA.WebApi.Models.TongHopVanBanQuyetDinhs;

namespace QLDA.WebApi.Controllers;

[Tags("In ấn")]
public class PrintController(IServiceProvider serviceProvider) : AggregateRootController(serviceProvider) {
    private readonly IUserProvider _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
    private readonly IExporterHelper _excelExporter = serviceProvider.GetRequiredService<IExporterHelper>();
    private readonly IAsposeHelper _asposeHelper = serviceProvider.GetRequiredService<IAsposeHelper>();
    private readonly IWordHelper _wordHelper = serviceProvider.GetRequiredService<IWordHelper>();

    /// <summary>
    /// Thêm timestamp vào tên file để tránh trùng khi tải nhiều lần
    /// </summary>
    private static string GetDownloadFileName(string templateFileName) {
        var nameWithoutExt = Path.GetFileNameWithoutExtension(templateFileName);
        var ext = Path.GetExtension(templateFileName);
        return $"{nameWithoutExt}_{DateTime.Now:ddMMyyyy_HHmmss}{ext}";
    }

    #region usp_In_QuyTrinhTrinhDuAn

    /// <summary>
    /// Export Quy trình trình dự án to Excel with N-level tree grouping
    /// Uses Level property for hierarchical outline, sorted by Stt
    /// </summary>
    [HttpGet("api/print/quy-trinh-trinh-du-an")]
    public async Task<IActionResult> InQuyTrinhTrinhDuAn([FromQuery] Guid duAnId, [FromQuery] bool includeHierarchicalStt = true) {
        var fileNameTemplate = "QuyTrinhTrinhDuAn.xlsx";
        var templatePath = Path.Combine(
            AppContext.BaseDirectory,
            "PrintTemplates",
            fileNameTemplate
        );

        ManagedException.ThrowIf(!System.IO.File.Exists(templatePath), "Không tìm thấy file template QuyTrinhTrinhDuAn.xlsx");

        var data = await Mediator.Send(new DuAnBuocGetTreeListQuery {
            DuAnId = duAnId
        });

        var firstRow = data.FirstOrDefault();
        ManagedException.ThrowIf(firstRow == null, "Không tìm thấy dữ liệu quy trình trình dự án");

        var exportResult = _excelExporter.ExportWithOutline(new TreeOutlineInstruction<DuAnBuocStateDto> {
            TemplatePath = templatePath,
            Items = data,
            LevelPropertyName = "Level",
            CollapseGroups = true,
            PlaceholderReplacements = new Dictionary<string, string> {
                { "$TenQuyTrinh", "[b]Quy trình:[/b] " + firstRow.TenQuyTrinh },
                { "$TenDuAn", "[b]Dự án:[/b] " + firstRow.TenDuAn }
            }
        });

        return new FileContentResult(exportResult.FileBytes, exportResult.ContentType) {
            FileDownloadName = GetDownloadFileName(fileNameTemplate)
        };
    }

    #endregion
    #region usp_In_DanhSach_DuAn_TraCuu

    /// <summary>
    /// usp_In_DanhSach_DuAn_TraCuu - DanhSachDuAnTraCuu.xlsx
    /// </summary>
    /// <param name="searchDto"></param>
    /// <returns></returns>
    [HttpGet("api/print/danh-sach-tra-cuu-du-an")]
    [ProducesResponseType<ResultApi<FileContentResult>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> InDuAnTraCuu([FromQuery] DuAnPrintSearchDto searchDto) {
        var fileNameTemplate = "DanhSachDuAnTraCuu.xlsx";
        var procedureName = "usp_In_DanhSach_DuAn_TraCuu";
        var templatePath = Path.Combine(
            AppContext.BaseDirectory, // ví dụ: ...\QLDA.WebApi
            "PrintTemplates", // chính xác tên folder trong project
            fileNameTemplate
        );

        ManagedException.ThrowIf(!System.IO.File.Exists(templatePath), "Không tìm thấy file template");

        var query = new GetStoreQuery() {
            PathTemplate = templatePath,
            ProcName = procedureName,
            Params = new {
                searchDto.TenDuAn,
                searchDto.MaDuAn,
                searchDto.ThoiGianKhoiCong,
                searchDto.ThoiGianHoanThanh,
                searchDto.MaNganSach,
                searchDto.LinhVucId,
                searchDto.NhomDuAnId,
                searchDto.LoaiDuAnId,
                DonViChinhId = searchDto.DonViPhuTrachChinhId,
                searchDto.DonViPhoiHopId,
                searchDto.LanhDaoPhuTrachId,
                searchDto.GiaiDoanId,
                searchDto.BuocId,
                searchDto.NguonVonId,
                searchDto.GlobalFilter,
                PageIndex = 0,
                PageSize = 0,
                searchDto.QuyTrinhId,
                searchDto.TrangThaiDuAnId,
            },
            HiddenColumns = searchDto.HiddenColumns
        };
        var exportResult = await Mediator.Send(query);

        return new FileContentResult(exportResult.FileBytes,
            exportResult.ContentType) {
            FileDownloadName = GetDownloadFileName(fileNameTemplate)
        };
    }

    #endregion

    #region usp_In_DanhSach_DuAn

    /// <summary>
    /// usp_In_DanhSach_DuAn - DanhSachDuAn.xlsx
    /// </summary>
    /// <param name="searchDto"></param>
    /// <returns></returns>
    [HttpGet("api/print/danh-sach-du-an")]
    public async Task<IActionResult> InDuAn([FromQuery] DuAnPrintSearchDto searchDto) {
        var fileNameTemplate = "DanhSachDuAn.xlsx";
        var procedureName = "usp_In_DanhSach_DuAn";
        var templatePath = Path.Combine(
            AppContext.BaseDirectory, // ví dụ: ...\QLDA.WebApi
            "PrintTemplates", // chính xác tên folder trong project
            fileNameTemplate
        );

        ManagedException.ThrowIf(!System.IO.File.Exists(templatePath), "Không tìm thấy file template");

        var query = new GetStoreQuery() {
            PathTemplate = templatePath,
            ProcName = procedureName,
            Params = new {
                searchDto.TenDuAn,
                searchDto.MaDuAn,
                searchDto.ThoiGianKhoiCong,
                searchDto.ThoiGianHoanThanh,
                searchDto.MaNganSach,
                searchDto.LinhVucId,
                searchDto.NhomDuAnId,
                searchDto.LoaiDuAnId,
                searchDto.DonViPhuTrachChinhId,
                searchDto.DonViPhoiHopId,
                searchDto.LanhDaoPhuTrachId,
                searchDto.GiaiDoanId,
                searchDto.BuocId,
                searchDto.NguonVonId,
                searchDto.GlobalFilter,
                PageIndex = 0,
                PageSize = 0,
                searchDto.QuyTrinhId,
                searchDto.TrangThaiDuAnId,
                TuNgay = searchDto.TuNgay?.ToStartOfDayUtc(),
                DenNgay = searchDto.DenNgay?.ToEndOfDayUtc(),
                searchDto.NamBatDau,
                searchDto.NamDuAn,
                searchDto.HinhThucDauTuId,
                searchDto.LoaiDuAnTheoNamId,
            },
            HiddenColumns = searchDto.HiddenColumns
        };
        var exportResult = await Mediator.Send(query);

        return new FileContentResult(exportResult.FileBytes,
            exportResult.ContentType) {
            FileDownloadName = GetDownloadFileName(fileNameTemplate)
        };
    }

    #endregion

    #region usp_In_DanhSach_GoiThau

    /// <summary>
    /// usp_In_DanhSach_GoiThau - DanhSachGoiThau.xlsx
    /// </summary>
    /// <param name="searchDto"></param>
    /// <returns></returns>
    [HttpGet("api/print/danh-sach-goi-thau")]
    public async Task<IActionResult> InGoiThau([FromQuery] GoiThauPrintSearchDto searchDto) {
        var fileNameTemplate = "DanhSachGoiThau.xlsx";
        var procedureName = "usp_In_DanhSach_GoiThau";
        var templatePath = Path.Combine(
            AppContext.BaseDirectory, // ví dụ: ...\QLDA.WebApi
            "PrintTemplates", // chính xác tên folder trong project
            fileNameTemplate
        );

        ManagedException.ThrowIf(!System.IO.File.Exists(templatePath), "Không tìm thấy file template");

        var query = new GetStoreQuery() {
            PathTemplate = templatePath,
            ProcName = procedureName,
            Params = new {
                searchDto.DuAnId,
                searchDto.BuocId,
                searchDto.GlobalFilter,
                searchDto.Ten,
                searchDto.HopDongId,
                searchDto.NguonVonId,
                searchDto.LoaiHopDongId,
                searchDto.LoaiGoiThauId,
                searchDto.PhuongThucLuaChonNhaThauId,
                searchDto.KeHoachLuaChonNhaThauId,
                searchDto.HinhThucLuaChonNhaThauId,
                searchDto.LoaiDuAnTheoNamId,
            },
            HiddenColumns = searchDto.HiddenColumns
        };
        var exportResult = await Mediator.Send(query);

        return new FileContentResult(exportResult.FileBytes,
            exportResult.ContentType) {
            FileDownloadName = GetDownloadFileName(fileNameTemplate)
        };
    }

    #endregion

    #region usp_In_DanhSach_HopDong

    /// <summary>
    /// usp_In_DanhSach_HopDong - DanhSachHopDong.xlsx
    /// </summary>
    /// <param name="searchDto"></param>
    /// <returns></returns>
    [HttpGet("api/print/danh-sach-hop-dong")]
    public async Task<IActionResult> InHopDong([FromQuery] HopDongPrintSearchDto searchDto) {
        var fileNameTemplate = "DanhSachHopDong.xlsx";
        var procedureName = "usp_In_DanhSach_HopDong";
        var templatePath = Path.Combine(
            AppContext.BaseDirectory, // ví dụ: ...\QLDA.WebApi
            "PrintTemplates", // chính xác tên folder trong project
            fileNameTemplate
        );

        ManagedException.ThrowIf(!System.IO.File.Exists(templatePath), "Không tìm thấy file template");

        var query = new GetStoreQuery() {
            PathTemplate = templatePath,
            ProcName = procedureName,
            Params = new {
                searchDto.DuAnId,
                searchDto.BuocId,
                searchDto.Ten,
                searchDto.SoHopDong,
                searchDto.NoiDung,
                searchDto.LoaiHopDongId,
                searchDto.DonViThucHienId,
                searchDto.IsBienBan,
                searchDto.GlobalFilter,
                searchDto.LoaiDuAnTheoNamId,
                PageIndex = 0,
                PageSize = 0,
            },
            HiddenColumns = searchDto.HiddenColumns
        };
        var exportResult = await Mediator.Send(query);

        return new FileContentResult(exportResult.FileBytes,
            exportResult.ContentType) {
            FileDownloadName = GetDownloadFileName(fileNameTemplate)
        };
    }

    #endregion

    #region usp_In_DanhSach_PhuLucHopDong
    /// <summary>
    /// usp_In_DanhSach_PhuLucHopDong - DanhSachPhuLucHopDong.xlsx
    /// </summary>
    /// <param name="searchModel"></param>
    /// <returns></returns>
    [HttpGet("api/print/danh-sach-phu-luc-hop-dong")]
    public async Task<IActionResult> InPhuLucHopDong([FromQuery] PhuLucHopDongPrintSearchModel searchModel) {
        var fileNameTemplate = "DanhSachPhuLucHopDong.xlsx";
        var procedureName = "usp_In_DanhSach_PhuLucHopDong";
        var templatePath = Path.Combine(
            AppContext.BaseDirectory, // ví dụ: ...\QLDA.WebApi
            "PrintTemplates", // chính xác tên folder trong project
            fileNameTemplate
        );

        ManagedException.ThrowIf(!System.IO.File.Exists(templatePath), "Không tìm thấy file template");

        var query = new GetStoreQuery() {
            PathTemplate = templatePath,
            ProcName = procedureName,
            Params = new {
                searchModel.DuAnId,
                searchModel.BuocId,
                searchModel.Ten,
                searchModel.SoPhuLucHopDong,
                searchModel.NoiDung,
                searchModel.HopDongId,
                TuNgay = searchModel.TuNgay?.ToStartOfDayUtc(),
                DenNgay = searchModel.DenNgay?.ToEndOfDayUtc(),
                searchModel.GlobalFilter,
                searchModel.LoaiDuAnTheoNamId,
                PageIndex = 0,
                PageSize = 0,
            },
            HiddenColumns = searchModel.HiddenColumns
        };
        var exportResult = await Mediator.Send(query);

        return new FileContentResult(exportResult.FileBytes,
            exportResult.ContentType) {
            FileDownloadName = GetDownloadFileName(fileNameTemplate)
        };
    }

    #endregion

    #region usp_In_DanhSach_BaoCaoTienDo

    /// <summary>
    /// usp_In_DanhSach_BaoCaoTienDo - DanhSachBaoCaoTienDo.xlsx
    /// </summary>
    /// <param name="searchModel"></param>
    /// <returns></returns>
    [HttpGet("api/print/danh-sach-bao-cao-tien-do")]
    public async Task<IActionResult> InBaoCaoTienDo([FromQuery] BaoCaoTienDoPrintSearchModel searchModel) {
        var fileNameTemplate = "DanhSachBaoCaoTienDo.xlsx";
        var procedureName = "usp_In_DanhSach_BaoCaoTienDo";
        var templatePath = Path.Combine(
            AppContext.BaseDirectory, // ví dụ: ...\QLDA.WebApi
            "PrintTemplates", // chính xác tên folder trong project
            fileNameTemplate
        );

        ManagedException.ThrowIf(!System.IO.File.Exists(templatePath), "Không tìm thấy file template");

        ManagedException.ThrowIf(_userProvider.Id == 0, "Vui lòng đăng nhập");
        var query = new GetStoreQuery() {
            PathTemplate = templatePath,
            ProcName = procedureName,
            Params = new {
                NguoiBaoCaoId = _userProvider.Id,
                searchModel.DuAnId,
                searchModel.BuocId,
                searchModel.NoiDung,
                searchModel.GlobalFilter,
                PageIndex = 0,
                PageSize = 0,
                TuNgay = searchModel.TuNgay?.ToStartOfDayUtc(),
                DenNgay = searchModel.DenNgay?.ToEndOfDayUtc(),
                searchModel.LoaiDuAnTheoNamId,
            },
            HiddenColumns = searchModel.HiddenColumns
        };
        var exportResult = await Mediator.Send(query);

        return new FileContentResult(exportResult.FileBytes,
            exportResult.ContentType) {
            FileDownloadName = GetDownloadFileName(fileNameTemplate)
        };
    }
    #endregion

    #region usp_In_DanhSach_BaoCaoBaoHanhSanPham

    /// <summary>
    /// usp_In_DanhSach_BaoCaoBaoHanhSanPham - DanhSachBaoCaoBaoHanhSanPham.xlsx
    /// </summary>
    /// <param name="searchModel"></param>
    /// <returns></returns>
    [HttpGet("api/print/danh-sach-bao-cao-bao-hanh-san-pham")]
    public async Task<IActionResult> InBaoCaoBaoHanhSanPham([FromQuery] BaoCaoBaoHanhSanPhamPrintSearchModel searchModel) {
        var fileNameTemplate = "DanhSachBaoCaoBaoHanhSanPham.xlsx";
        var procedureName = "usp_In_DanhSach_BaoCaoBaoHanhSanPham";
        var templatePath = Path.Combine(
            AppContext.BaseDirectory, // ví dụ: ...\QLDA.WebApi
            "PrintTemplates", // chính xác tên folder trong project
            fileNameTemplate
        );

        ManagedException.ThrowIf(!System.IO.File.Exists(templatePath), "Không tìm thấy file template");

        ManagedException.ThrowIf(_userProvider.Id == 0, "Vui lòng đăng nhập");
        var query = new GetStoreQuery() {
            PathTemplate = templatePath,
            ProcName = procedureName,
            Params = new {
                NguoiBaoCaoId = _userProvider.Id,
                searchModel.DuAnId,
                searchModel.BuocId,
                searchModel.NoiDung,
                TuNgay = searchModel.TuNgay?.ToStartOfDayUtc(),
                DenNgay = searchModel.DenNgay?.ToEndOfDayUtc(),
                searchModel.GlobalFilter,
                PageIndex = 0,
                PageSize = 0,
            },
            HiddenColumns = searchModel.HiddenColumns
        };
        var exportResult = await Mediator.Send(query);

        return new FileContentResult(exportResult.FileBytes,
            exportResult.ContentType) {
            FileDownloadName = GetDownloadFileName(fileNameTemplate)
        };
    }

    #endregion

    #region usp_In_DanhSach_BaoCaoBanGiaoSanPham


    /// <summary>
    /// usp_In_DanhSach_BaoCaoBanGiaoSanPham - DanhSachBaoCaoBanGiaoSanPham.xlsx
    /// </summary>
    /// <param name="searchModel"></param>
    /// <returns></returns>
    [HttpGet("api/print/danh-sach-bao-cao-ban-giao-san-pham")]
    public async Task<IActionResult> InBaoCaoBanGiaoSanPham([FromQuery] BaoCaoBanGiaoSanPhamPrintSearchModel searchModel) {
        var fileNameTemplate = "DanhSachBaoCaoBanGiaoSanPham.xlsx";
        var procedureName = "usp_In_DanhSach_BaoCaoBanGiaoSanPham";
        var templatePath = Path.Combine(
            AppContext.BaseDirectory, // ví dụ: ...\QLDA.WebApi
            "PrintTemplates", // chính xác tên folder trong project
            fileNameTemplate
        );

        ManagedException.ThrowIf(!System.IO.File.Exists(templatePath), "Không tìm thấy file template");

        ManagedException.ThrowIf(_userProvider.Id == 0, "Vui lòng đăng nhập");
        var query = new GetStoreQuery() {
            PathTemplate = templatePath,
            ProcName = procedureName,
            Params = new {
                NguoiBaoCaoId = _userProvider.Id,
                searchModel.DuAnId,
                searchModel.BuocId,
                searchModel.NoiDung,
                TuNgay = searchModel.TuNgay?.ToStartOfDayUtc(),
                DenNgay = searchModel.DenNgay?.ToEndOfDayUtc(),
                searchModel.GlobalFilter,
                searchModel.LoaiDuAnTheoNamId,
                PageIndex = 0,
                PageSize = 0,
            },
            HiddenColumns = searchModel.HiddenColumns
        };
        var exportResult = await Mediator.Send(query);

        return new FileContentResult(exportResult.FileBytes,
            exportResult.ContentType) {
            FileDownloadName = GetDownloadFileName(fileNameTemplate)
        };
    }

    #endregion

    #region usp_In_DanhSach_KhoKhanVuongMac

    /// <summary>
    /// usp_In_DanhSach_KhoKhanVuongMac - DanhSachKhoKhanVuongMac.xlsx
    /// </summary>
    /// <param name="searchModel"></param>
    /// <returns></returns>
    [HttpGet("api/print/danh-sach-kho-khan-vuong-mac")]
    public async Task<IActionResult> InKhoKhanVuongMac([FromQuery] KhoKhanVuongMacPrintSearchModel searchModel) {
        var fileNameTemplate = "DanhSachKhoKhanVuongMac.xlsx";
        var procedureName = "usp_In_DanhSach_KhoKhanVuongMac";
        var templatePath = Path.Combine(
            AppContext.BaseDirectory, // ví dụ: ...\QLDA.WebApi
            "PrintTemplates", // chính xác tên folder trong project
            fileNameTemplate
        );

        ManagedException.ThrowIf(!System.IO.File.Exists(templatePath), "Không tìm thấy file template");

        ManagedException.ThrowIf(_userProvider.Id == 0, "Vui lòng đăng nhập");
        var query = new GetStoreQuery() {
            PathTemplate = templatePath,
            ProcName = procedureName,
            Params = new {
                searchModel.DuAnId,
                searchModel.BuocId,
                searchModel.NoiDung,
                searchModel.TinhTrangId,
                searchModel.MucDoKhoKhanId,
                searchModel.LoaiDuAnId,
                searchModel.LanhDaoPhuTrachId,
                TuNgay = searchModel.TuNgay?.ToStartOfDayUtc(),
                DenNgay = searchModel.DenNgay?.ToEndOfDayUtc(),
                searchModel.GlobalFilter,
                searchModel.LoaiDuAnTheoNamId,
                PageIndex = 0,
                PageSize = 0,
            },
            HiddenColumns = searchModel.HiddenColumns
        };
        var exportResult = await Mediator.Send(query);

        return new FileContentResult(exportResult.FileBytes,
            exportResult.ContentType) {
            FileDownloadName = GetDownloadFileName(fileNameTemplate)
        };
    }

    #endregion

    #region usp_In_DanhSach_TongHopVanBanQuyetDinh

    /// <summary>
    /// usp_In_DanhSach_TongHopVanBanQuyetDinh - DanhSachTongHopVanBanQuyetDinh.xlsx
    /// </summary>
    /// <param name="searchModel"></param>
    /// <returns></returns>
    [HttpGet("api/print/danh-sach-tong-hop-van-ban-quyet-dinh")]
    public async Task<IActionResult>
        InTongHopVanBanQuyetDinh([FromQuery] TongHopVanBanQuyetDinhPrintSearchModel searchModel) {
        var fileNameTemplate = "DanhSachTongHopVanBanQuyetDinh.xlsx";
        var procedureName = "usp_In_DanhSach_TongHopVanBanQuyetDinh";
        var templatePath = Path.Combine(
            AppContext.BaseDirectory, // ví dụ: ...\QLDA.WebApi
            "PrintTemplates", // chính xác tên folder trong project
            fileNameTemplate
        );

        ManagedException.ThrowIf(!System.IO.File.Exists(templatePath), "Không tìm thấy file template");

        ManagedException.ThrowIf(_userProvider.Id == 0, "Vui lòng đăng nhập");
        var query = new GetStoreQuery() {
            PathTemplate = templatePath,
            ProcName = procedureName,
            Params = new {
                searchModel.DuAnId,
                searchModel.BuocId,
                MaELoaiVanBanQuyetDinh = searchModel.Loai?.ToString(),
                searchModel.TrichYeu,
                TuNgay = searchModel.TuNgay?.ToStartOfDayUtc(),
                DenNgay = searchModel.DenNgay?.ToEndOfDayUtc(),
                searchModel.GlobalFilter,
                searchModel.LoaiDuAnTheoNamId,
                PageIndex = 0,
                PageSize = 0,
            },
            HiddenColumns = searchModel.HiddenColumns
        };
        var exportResult = await Mediator.Send(query);

        return new FileContentResult(exportResult.FileBytes,
            exportResult.ContentType) {
            FileDownloadName = GetDownloadFileName(fileNameTemplate)
        };
    }

    #endregion

    #region usp_In_DanhSachTreHanPhongBan

    /// <summary>
    /// usp_In_DanhSachTreHanPhongBan - DanhSachTreHanBuocPhongBan.xlsx
    /// </summary>
    /// <returns></returns>
    [HttpGet("api/print/danh-sach-tre-han-phong-ban")]
    public async Task<IActionResult>
        InDanhSachTreHanPhongBan() {
        var fileNameTemplate = "DanhSachTreHanBuocPhongBan.xlsx";
        var procedureName = "usp_In_DanhSachTreHanPhongBan";
        var templatePath = Path.Combine(
            AppContext.BaseDirectory, // ví dụ: ...\QLDA.WebApi
            "PrintTemplates", // chính xác tên folder trong project
            fileNameTemplate
        );

        ManagedException.ThrowIf(!System.IO.File.Exists(templatePath), "Không tìm thấy file template");

        ManagedException.ThrowIf(_userProvider.Id == 0, "Vui lòng đăng nhập");
        var query = new GetStoreQuery() {
            PathTemplate = templatePath,
            ProcName = procedureName,
            Params = null,
            HiddenColumns = []
        };
        var exportResult = await Mediator.Send(query);

        return new FileContentResult(exportResult.FileBytes,
            exportResult.ContentType) {
            FileDownloadName = GetDownloadFileName(fileNameTemplate)
        };
    }

    #endregion

    #region DanhSachPhanKhaiKinhPhi

    /// <summary>
    /// DanhSachPhanKhaiKinhPhi.xlsx — Export danh sách phân khai kinh phí (theo filter grid)
    /// </summary>
    [HttpGet("api/print/danh-sach-phan-khai-kinh-phi")]
    [Authorize(Roles = RoleConstants.GroupPhanKhaiKinhPhiExport)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> InDanhSachPhanKhaiKinhPhi(
        [FromQuery] PhanKhaiKinhPhiPrintSearchModel searchModel,
        CancellationToken cancellationToken = default) {
        var fileNameTemplate = "DanhSachPhanKhaiKinhPhi.xlsx";
        var templatePath = Path.Combine(
            AppContext.BaseDirectory,
            "PrintTemplates",
            fileNameTemplate
        );

        ManagedException.ThrowIf(!System.IO.File.Exists(templatePath), "Không tìm thấy file template");
        ManagedException.ThrowIf(_userProvider.Id == 0, "Vui lòng đăng nhập");

        var data = await Mediator.Send(new PhanKhaiKinhPhiGetDanhSachExportQuery {
            DuAnId = searchModel.DuAnId,
            GlobalFilter = searchModel.GlobalFilter,
            TrangThaiId = searchModel.TrangThaiId,
        }, cancellationToken);

        var exportResult = _excelExporter.Export(new AsposeInstruction<PhanKhaiKinhPhiDanhSachExportDto> {
            TemplatePath = templatePath,
            Items = data,
            HiddenColumns = searchModel.HiddenColumns ?? [],
            AutoFitColumnsAndRows = false,
        });

        var downloadName = $"PhanKhaiKinhPhi_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

        return new FileContentResult(exportResult.FileBytes, exportResult.ContentType) {
            FileDownloadName = downloadName
        };
    }

    #endregion

    #region KetQuaPhanKhaiVonDuocDuyet

    /// <summary>
    /// KetQuaPhanKhaiVonDuocDuyet.xlsx — Export kết quả phân khai vốn đã duyệt
    /// </summary>
    [HttpGet("api/print/ket-qua-phan-khai-von-duoc-duyet")]
    [Authorize(Roles = RoleConstants.GroupPhanKhaiKinhPhiExport)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> InKetQuaPhanKhaiVonDuocDuyet([FromQuery] PhanKhaiKinhPhiPrintSearchModel searchModel) {
        var fileNameTemplate = "KetQuaPhanKhaiVonDuocDuyet.xlsx";
        var templatePath = Path.Combine(
            AppContext.BaseDirectory,
            "PrintTemplates",
            fileNameTemplate
        );

        ManagedException.ThrowIf(!System.IO.File.Exists(templatePath), "Không tìm thấy file template");
        ManagedException.ThrowIf(_userProvider.Id == 0, "Vui lòng đăng nhập");

        var data = await Mediator.Send(new PhanKhaiKinhPhiGetDanhSachDaDuyetExportQuery {
            DuAnId = searchModel.DuAnId,
            GlobalFilter = searchModel.GlobalFilter,
        });

        var exportResult = _excelExporter.Export(new AsposeInstruction<PhanKhaiKinhPhiExportDto> {
            TemplatePath = templatePath,
            Items = data,
            HiddenColumns = searchModel.HiddenColumns ?? [],
            AutoFitColumnsAndRows = false,
        });

        return new FileContentResult(exportResult.FileBytes, exportResult.ContentType) {
            FileDownloadName = GetDownloadFileName(fileNameTemplate)
        };
    }

    #endregion

    #region DanhSachDeXuatChuTruongChuyenTiep

    /// <summary>
    /// DanhSachDeXuatChuTruongChuyenTiep.xlsx — Export danh sách đề xuất chủ trương chuyển tiếp
    /// </summary>
    [HttpGet("api/print/danh-sach-de-xuat-chu-truong-chuyen-tiep")]
    [Authorize(Roles = RoleConstants.GroupDeXuatChuTruongChuyenTiepExport)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> InDanhSachDeXuatChuTruongChuyenTiep(
        [FromQuery] DeXuatChuyenTiepPrintSearchModel searchModel) {
        var fileNameTemplate = "DanhSachDeXuatChuTruongChuyenTiep.xlsx";
        var templatePath = Path.Combine(
            AppContext.BaseDirectory,
            "PrintTemplates",
            fileNameTemplate
        );

        ManagedException.ThrowIf(!System.IO.File.Exists(templatePath), "Không tìm thấy file template");
        ManagedException.ThrowIf(_userProvider.Id == 0, "Vui lòng đăng nhập");

        var data = await Mediator.Send(new DeXuatChuyenTiepGetDanhSachExportQuery {
            DuAnId = searchModel.DuAnId,
            BuocId = searchModel.BuocId,
        });

        var exportResult = _excelExporter.Export(new AsposeInstruction<DeXuatChuyenTiepExportDto> {
            TemplatePath = templatePath,
            Items = data,
            HiddenColumns = searchModel.HiddenColumns ?? [],
            AutoFitColumnsAndRows = false,
        });

        return new FileContentResult(exportResult.FileBytes, exportResult.ContentType) {
            FileDownloadName = GetDownloadFileName(fileNameTemplate)
        };
    }

    #endregion

    #region DanhSachXinChuTruongDauTu

    /// <summary>
    /// DanhSachXinChuTruongDauTu.xlsx — Export danh sách xin chủ trương đầu tư
    /// </summary>
    [HttpGet("api/print/danh-sach-xin-chu-truong-dau-tu")]
    [Authorize(Roles = RoleConstants.GroupXinChuTruongDauTuExport)]
    [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> InDanhSachXinChuTruongDauTu(
        [FromQuery] DeXuatNhuCauKinhPhiPrintSearchModel searchModel) {
        var fileNameTemplate = "DanhSachXinChuTruongDauTu.xlsx";
        var templatePath = Path.Combine(
            AppContext.BaseDirectory,
            "PrintTemplates",
            fileNameTemplate
        );

        ManagedException.ThrowIf(!System.IO.File.Exists(templatePath), "Không tìm thấy file template");
        ManagedException.ThrowIf(_userProvider.Id == 0, "Vui lòng đăng nhập");

        var data = await Mediator.Send(new DeXuatNhuCauKinhPhiGetDanhSachExportQuery {
            DuAnId = searchModel.DuAnId,
            TrangThaiId = searchModel.TrangThaiId,
            GlobalFilter = searchModel.GlobalFilter,
        });

        var exportResult = _excelExporter.Export(new AsposeInstruction<DeXuatNhuCauKinhPhiExportDto> {
            TemplatePath = templatePath,
            Items = data,
            HiddenColumns = searchModel.HiddenColumns ?? [],
            AutoFitColumnsAndRows = false,
        });

        return new FileContentResult(exportResult.FileBytes, exportResult.ContentType) {
            FileDownloadName = GetDownloadFileName(fileNameTemplate)
        };
    }

    #endregion

    #region TongHopNhuCauKinhPhiNam

    /// <summary>
    /// TinhHinhDeXuatNhuCau.xlsx — Export tổng hợp nhu cầu kinh phí năm
    /// </summary>
    [HttpGet("api/print/tinh-hinh-de-xuat-nhu-cau")]
    [Authorize(Roles = RoleConstants.GroupTinhHinhDeXuatNhuCauExport)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> InTinhHinhDeXuatNhuCau(
        [FromQuery] TheoDoiDeXuatNhuCauKinhPhiPrintSearchModel searchModel) {
        var fileNameTemplate = "TinhHinhDeXuatNhuCau.xlsx";
        var templatePath = Path.Combine(
            AppContext.BaseDirectory,
            "PrintTemplates",
            fileNameTemplate
        );

        ManagedException.ThrowIf(!System.IO.File.Exists(templatePath), "Không tìm thấy file template");
        ManagedException.ThrowIf(_userProvider.Id == 0, "Vui lòng đăng nhập");

        var data = await Mediator.Send(new TheoDoiDeXuatNhuCauKinhPhiGetExportQuery {
            DuAnId = searchModel.DuAnId,
            TrangThaiId = searchModel.TrangThaiId,
            TrangThaiKeHoachId = searchModel.TrangThaiKeHoachNamId,
            SoPhieuChuyen = searchModel.SoPhieuChuyen,
            TrichYeu = searchModel.TrichYeu,
            GlobalFilter = searchModel.GlobalFilter,
            TuNgay = searchModel.TuNgay,
            DenNgay = searchModel.DenNgay,
            DonViDeXuatId = searchModel.DonViDeXuatId,
        });

        var exportResult = _excelExporter.Export(new AsposeInstruction<TongHopNhuCauKinhPhiNamExportDto> {
            TemplatePath = templatePath,
            Items = data,
            HiddenColumns = searchModel.HiddenColumns ?? [],
            AutoFitColumnsAndRows = false,
        });

        return new FileContentResult(exportResult.FileBytes, exportResult.ContentType) {
            FileDownloadName = GetDownloadFileName(fileNameTemplate)
        };
    }

    #endregion

    #region BaoCaoDeXuatChuTruong

    /// <summary>
    /// BaoCaoDeXuatChuTruong.xlsx — Export báo cáo đề xuất chủ trương
    /// </summary>
    [HttpGet("api/print/bao-cao-de-xuat-chu-truong")]
    [Authorize(Roles = RoleConstants.GroupBaoCaoDeXuatChuTruongExport)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> InBaoCaoDeXuatChuTruong(
        [FromQuery] TongHopDeXuatChuTruongPrintSearchModel searchModel) {
        var fileNameTemplate = "BaoCaoDeXuatChuTruong.xlsx";
        var templatePath = Path.Combine(
            AppContext.BaseDirectory,
            "PrintTemplates",
            fileNameTemplate
        );

        ManagedException.ThrowIf(!System.IO.File.Exists(templatePath), "Không tìm thấy file template");
        ManagedException.ThrowIf(_userProvider.Id == 0, "Vui lòng đăng nhập");

        var result = await Mediator.Send(new TongHopDeXuatChuTruongGetExportQuery {
            DuAnId = searchModel.DuAnId,
            BuocId = searchModel.BuocId,
            GlobalFilter = searchModel.GlobalFilter,
            Loai = searchModel.Loai,
            Nam = searchModel.Nam,
            DonViPhuTrachId = searchModel.DonViPhuTrachId,
        });

        var preparedTemplatePath = ExcelExportTemplateHelper.PrepareTemplateWithPlaceholders(
            _asposeHelper,
            templatePath,
            new Dictionary<string, string> {
                { "$TongSoDeXuat", result.TongSoDeXuat.ToString() },
                { "$TongChuTruongMoi", result.TongDeXuatMoi.ToString() },
                { "$TongChuyenTiep", result.TongDeXuatChuyenTiep.ToString() },
            });

        try {
            var exportResult = _excelExporter.Export(new AsposeInstruction<TongHopDeXuatChuTruongExportDto> {
                TemplatePath = preparedTemplatePath,
                Items = result.Rows,
                HiddenColumns = searchModel.HiddenColumns ?? [],
                AutoFitColumnsAndRows = false,
            });

            return new FileContentResult(exportResult.FileBytes, exportResult.ContentType) {
                FileDownloadName = GetDownloadFileName(fileNameTemplate)
            };
        } finally {
            if (System.IO.File.Exists(preparedTemplatePath)) {
                System.IO.File.Delete(preparedTemplatePath);
            }
        }
    }

    #endregion

    [HttpGet("api/print/bao-cao-tien-do-du-an")]
    public async Task<IActionResult> InBaoCaoTienDoDuAn([FromQuery] BaoCaoDuAnSearchDto searchModel)
    {
        var fileNameTemplate = "BaoCaoTienDoDuAn.xlsx";
        var procedureName = "usp_In_BaoCao_TienDo_DuAn";
        var templatePath = Path.Combine(
            AppContext.BaseDirectory, // ví dụ: ...\QLDA.WebApi
            "PrintTemplates", // chính xác tên folder trong project
            fileNameTemplate
        );

        ManagedException.ThrowIf(!System.IO.File.Exists(templatePath), "Không tìm thấy file template");

        ManagedException.ThrowIf(_userProvider.Id == 0, "Vui lòng đăng nhập");

        
        var query = new GetStoreQuery()
        {
            PathTemplate = templatePath,
            ProcName = procedureName,
            Params  = new
            {
                searchModel.LoaiDuAnTheoNamId,
                searchModel.LoaiDuAnId,
                searchModel.TenDuAn,
                searchModel.ThoiGianKhoiCong,
                searchModel.ThoiGianHoanThanh,
                searchModel.HinhThucDauTuId,
                searchModel.DonViPhuTrachChinhId,
            },
            HiddenColumns = []
        };
        var exportResult = await Mediator.Send(query);

        return new FileContentResult(exportResult.FileBytes,    exportResult.ContentType)
        {
            FileDownloadName = GetDownloadFileName(fileNameTemplate)
        };
    }

    #region In_BienBanBanGiao_HoSo

    /// <summary>
    /// Export Biên Bản Bàn Giao Hồ Sơ ra file .docx
    /// </summary>
    /// <param name="id">BanGiaoHoSo Id</param>
    [HttpGet("api/print/bien-ban-ban-giao-ho-so")]
    [ProducesResponseType<ResultApi<FileContentResult>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> InBienBanBanGiaoHoSo([FromQuery] Guid id) {
        var fileNameTemplate = "BienBanBanGiao.docx";
        var templatePath = Path.Combine(
            AppContext.BaseDirectory,
            "PrintTemplates",
            "Word",
            fileNameTemplate
        );

        ManagedException.ThrowIf(!System.IO.File.Exists(templatePath), "Không tìm thấy file template BienBanBanGiao.docx");

        var dto = await Mediator.Send(new BanGiaoHoSoPrintQuery(id));

        var replacements = new Dictionary<string, string> {
            { "ngay", dto.NgayBanGiao.HasValue
                ? $"ngày {dto.NgayBanGiao.Value:dd} tháng {dto.NgayBanGiao.Value:MM} năm {dto.NgayBanGiao.Value:yyyy}"
                : "" },
            { "PhongBanChuTri", dto.TenPhongBanChuTri ?? "" },
            { "PhongBanNhanHoSo", dto.TenPhongBanNhan ?? "" },
            { "maHoSo", dto.Ma ?? "" },
            { "tenHoSo", dto.TenHoSo ?? "" },
            { "TenDuAn", dto.TenDuAn ?? "" },
            { "maLuuTru", dto.MaDuAn ?? "" },
            { "TongSoTepDinhKem", dto.TongSoTepDinhKem.ToString() },
            { "ghiChu", dto.GhiChu ?? "" }
        };

        var bytes = _wordHelper.ExportFromTemplate(templatePath, replacements);

        return File(bytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            GetDownloadFileName(fileNameTemplate));
    }

    #endregion

}
