using QLDA.Application.DuAns.DTOs;
using QLDA.Application.DuAnBuocs.Queries;
using QLDA.Application.GoiThaus.DTOs;
using QLDA.Application.HopDongs.DTOs;
using QLDA.WebApi.Models.BaoCaoBanGiaoSanPhams;
using QLDA.WebApi.Models.BaoCaoBaoHanhSanPhams;
using QLDA.WebApi.Models.BaoCaoTienDos;
using QLDA.WebApi.Models.KhoKhanVuongMacs;
using QLDA.WebApi.Models.PhuLucHopDongs;
using QLDA.WebApi.Models.TongHopVanBanQuyetDinhs;
using QLDA.Application.DuAnBuocs.DTOs;

namespace QLDA.WebApi.Controllers;

[Tags("In ấn")]
public class PrintController(IServiceProvider serviceProvider) : AggregateRootController(serviceProvider) {
    private readonly IUserProvider _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
    private readonly IExporterHelper _excelExporter = serviceProvider.GetRequiredService<IExporterHelper>();

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
            FileDownloadName = $"QuyTrinhTrinhDuAn_{DateTime.Now:ddMMyyyy_HHmmss}.xlsx"
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
    public async Task<IActionResult> InDuAnTraCuu([FromQuery] DuAnSearchDto searchDto) {
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
                searchDto.PageIndex,
                searchDto.PageSize,
                searchDto.QuyTrinhId,
                searchDto.TrangThaiDuAnId,
            },
            HiddenColumns = searchDto.HiddenColumns
        };
        var exportResult = await Mediator.Send(query);

        return new FileContentResult(exportResult.FileBytes,
            exportResult.ContentType) {
            FileDownloadName = fileNameTemplate
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
    public async Task<IActionResult> InDuAn([FromQuery] DuAnSearchDto searchDto) {
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
                searchDto.PageIndex,
                searchDto.PageSize,
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
            FileDownloadName = fileNameTemplate
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
    public async Task<IActionResult> InGoiThau([FromQuery] GoiThauSearchDto searchDto) {
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
            },
            HiddenColumns = searchDto.HiddenColumns
        };
        var exportResult = await Mediator.Send(query);

        return new FileContentResult(exportResult.FileBytes,
            exportResult.ContentType) {
            FileDownloadName = fileNameTemplate
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
    public async Task<IActionResult> InHopDong([FromQuery] HopDongSearchDto searchDto) {
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
                searchDto.PageIndex,
                searchDto.PageSize,
            },
            HiddenColumns = searchDto.HiddenColumns
        };
        var exportResult = await Mediator.Send(query);

        return new FileContentResult(exportResult.FileBytes,
            exportResult.ContentType) {
            FileDownloadName = fileNameTemplate
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
    public async Task<IActionResult> InPhuLucHopDong([FromQuery] PhuLucHopDongSearchModel searchModel) {
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
                searchModel.PageIndex,
                searchModel.PageSize,
            },
            HiddenColumns = searchModel.HiddenColumns
        };
        var exportResult = await Mediator.Send(query);

        return new FileContentResult(exportResult.FileBytes,
            exportResult.ContentType) {
            FileDownloadName = fileNameTemplate
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
    public async Task<IActionResult> InBaoCaoTienDo([FromQuery] BaoCaoTienDoSearchModel searchModel) {
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
                searchModel.PageIndex,
                searchModel.PageSize,
                TuNgay = searchModel.TuNgay?.ToStartOfDayUtc(),
                DenNgay = searchModel.DenNgay?.ToEndOfDayUtc(),
            },
            HiddenColumns = searchModel.HiddenColumns
        };
        var exportResult = await Mediator.Send(query);

        return new FileContentResult(exportResult.FileBytes,
            exportResult.ContentType) {
            FileDownloadName = fileNameTemplate
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
    public async Task<IActionResult> InBaoCaoBaoHanhSanPham([FromQuery] BaoCaoBaoHanhSanPhamSearchModel searchModel) {
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
                searchModel.PageIndex,
                searchModel.PageSize,
            },
            HiddenColumns = searchModel.HiddenColumns
        };
        var exportResult = await Mediator.Send(query);

        return new FileContentResult(exportResult.FileBytes,
            exportResult.ContentType) {
            FileDownloadName = fileNameTemplate
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
    public async Task<IActionResult> InBaoCaoBanGiaoSanPham([FromQuery] BaoCaoBanGiaoSanPhamSearchModel searchModel) {
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
                searchModel.PageIndex,
                searchModel.PageSize,
            },
            HiddenColumns = searchModel.HiddenColumns
        };
        var exportResult = await Mediator.Send(query);

        return new FileContentResult(exportResult.FileBytes,
            exportResult.ContentType) {
            FileDownloadName = fileNameTemplate
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
    public async Task<IActionResult> InKhoKhanVuongMac([FromQuery] KhoKhanVuongMacSearchModel searchModel) {
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
                searchModel.PageIndex,
                searchModel.PageSize,
            },
            HiddenColumns = searchModel.HiddenColumns
        };
        var exportResult = await Mediator.Send(query);

        return new FileContentResult(exportResult.FileBytes,
            exportResult.ContentType) {
            FileDownloadName = fileNameTemplate
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
        InTongHopVanBanQuyetDinh([FromQuery] TongHopVanBanQuyetDinhSearchModel searchModel) {
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
                searchModel.PageIndex,
                searchModel.PageSize,
            },
            HiddenColumns = searchModel.HiddenColumns
        };
        var exportResult = await Mediator.Send(query);

        return new FileContentResult(exportResult.FileBytes,
            exportResult.ContentType) {
            FileDownloadName = fileNameTemplate
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
            FileDownloadName = fileNameTemplate
        };
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
            FileDownloadName = fileNameTemplate
        };
    }

}
