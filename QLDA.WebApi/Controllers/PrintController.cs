using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using BuildingBlocks.Application.Common.Converters;
using BuildingBlocks.CrossCutting.Offices;
using BuildingBlocks.Infrastructure.Offices;
using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using QLDA.Application.BanGiaoHoSos.DTOs;
using QLDA.Application.BanGiaoHoSos.Queries;
using QLDA.Application.DeXuatChuyenTieps.DTOs;
using QLDA.Application.DeXuatChuyenTieps.Queries;
using QLDA.Application.DeXuatNhuCauKinhPhiNams.DTOs;
using QLDA.Application.DeXuatNhuCauKinhPhiNams.Queries;
using QLDA.Application.DeXuatNhuCauKinhPhis.DTOs;
using QLDA.Application.DeXuatNhuCauKinhPhis.Queries;
using QLDA.Application.DuAnBuocs.DTOs;
using QLDA.Application.DuAnBuocs.Queries;
using QLDA.Application.DuAns.DTOs;
using QLDA.Application.DuAns.Queries;
using QLDA.Application.GoiThaus.DTOs;
using QLDA.Application.GoiThaus.Queries;
using QLDA.Application.HopDongs.DTOs;
using QLDA.Application.KeHoachTrienKhaiHangMucs.DTOs;
using QLDA.Application.KeHoachTrienKhaiHangMucs.Queries;
using QLDA.Application.KhoKhanVuongMacs.DTOs;
using QLDA.Application.KhoKhanVuongMacs.Queries;
using QLDA.Application.KySos.DTOs;
using QLDA.Application.KySos.Queries;
using QLDA.Application.PhanKhaiKinhPhis.DTOs;
using QLDA.Application.PhanKhaiKinhPhis.Queries;
using QLDA.Application.QuanLyPheDuyet;
using QLDA.Application.QuanLyPheDuyet.DTOs;
using QLDA.Application.QuanLyPheDuyet.Queries;
using QLDA.Application.QuyetDinhLapBanQLDAs.Queries;
using QLDA.Application.TongHopDeXuatChuTruongs.DTOs;
using QLDA.Application.TongHopDeXuatChuTruongs.Queries;
using QLDA.Application.TongHopVanBanQuyetDinhs.DTOs;
using QLDA.Application.TongHopVanBanQuyetDinhs.Queries;
using QLDA.Application.ToTrinhPheDuyets.Queries;
using QLDA.Application.TrienKhaiKeHoachLCNTs.DTOs;
using QLDA.Application.TrienKhaiKeHoachLCNTs.Queries;
using QLDA.Domain.Constants;
using QLDA.Domain.Interfaces;
using QLDA.Infrastructure.Offices;
using QLDA.WebApi.Models.BaoCaoBanGiaoSanPhams;
using QLDA.WebApi.Models.BaoCaoBaoHanhSanPhams;
using QLDA.WebApi.Models.BaoCaoTienDos;
using QLDA.WebApi.Models.DeXuatChuTruongChuyenTieps;
using QLDA.WebApi.Models.DeXuatNhuCauKinhPhiNams;
using QLDA.WebApi.Models.DeXuatNhuCauKinhPhis;
using QLDA.WebApi.Models.KhoKhanVuongMacs;
using QLDA.WebApi.Models.PhanKhaiKinhPhis;
using QLDA.WebApi.Models.PhuLucHopDongs;
using QLDA.WebApi.Models.TongHopDeXuatChuTruongs;
using QLDA.WebApi.Models.TongHopVanBanQuyetDinhs;
using Serilog;


namespace QLDA.WebApi.Controllers;

[Tags("In ấn")]
public class PrintController(IServiceProvider serviceProvider) : AggregateRootController(serviceProvider) {
    private readonly IUserProvider _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
    private readonly IExporterHelper _excelExporter = serviceProvider.GetRequiredService<IExporterHelper>();
    private readonly IAsposeHelper _asposeHelper = serviceProvider.GetRequiredService<IAsposeHelper>();
    private readonly IWordHelper _wordHelper = serviceProvider.GetRequiredService<IWordHelper>();
    private readonly KeHoachTrienKhaiHangMucWordExporter _keHoachWordExporter =
        serviceProvider.GetRequiredService<KeHoachTrienKhaiHangMucWordExporter>();

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

        // Remap Level<=0 → Level=1 (tránh Stack empty pop trong ExportWithOutline)
        // và convert 4 cột ngày thành string dd/MM/yyyy để file in chỉ hiển thị ngày.
        var printData = data.Select(x => new DuAnBuocStatePrintRow {
            Id = x.Id,
            TenDuAn = x.TenDuAn,
            QuyTrinhId = x.QuyTrinhId,
            TenQuyTrinh = x.TenQuyTrinh,
            GiaiDoanId = x.GiaiDoanId,
            TenGiaiDoan = x.TenGiaiDoan,
            BuocId = x.BuocId,
            TenBuoc = x.TenBuoc,
            PartialView = x.PartialView,
            ParentId = x.ParentId,
            Path = x.Path,
            Level = x.Level <= 0 ? 1 : x.Level,
            Stt = x.Stt,
            HierarchicalStt = x.HierarchicalStt,
            TrangThaiId = x.TrangThaiId,
            NgayDuKienBatDau = x.NgayDuKienBatDau?.ToDateOnlyVn().ToString("dd/MM/yyyy"),
            NgayDuKienKetThuc = x.NgayDuKienKetThuc?.ToDateOnlyVn().ToString("dd/MM/yyyy"),
            NgayThucTeBatDau = x.NgayThucTeBatDau?.ToDateOnlyVn().ToString("dd/MM/yyyy"),
            NgayThucTeKetThuc = x.NgayThucTeKetThuc?.ToDateOnlyVn().ToString("dd/MM/yyyy"),
            GhiChu = x.GhiChu,
            TrachNhiemThucHien = x.TrachNhiemThucHien,
            IsKetThuc = x.IsKetThuc,
            PhongPhuTrachChinhId = x.PhongPhuTrachChinhId,
            PhongBanPhuTrachChinh = x.PhongBanPhuTrachChinh,
            DanhSachPhongBanPhoiHops = x.DanhSachPhongBanPhoiHops,
        }).ToList();

        var exportResult = _excelExporter.ExportWithOutline(new TreeOutlineInstruction<DuAnBuocStatePrintRow> {
            TemplatePath = templatePath,
            Items = printData,
            LevelPropertyName = "Level",
            CollapseGroups = true,
            PlaceholderReplacements = new Dictionary<string, string> {
                { "$TenQuyTrinh", "Quy trình: " + firstRow.TenQuyTrinh },
                { "$TenDuAn", "Dự án: " + firstRow.TenDuAn }
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

    #region DanhSachDuAn (Aspose export)

    /// <summary>
    /// DanhSachDuAn.xlsx — Export danh sách dự án ra Excel dùng Aspose.
    /// Dữ liệu lấy qua DuAnGetDanhSachExportQuery (cùng filter set như DuAnGetDanhSachQuery).
    /// </summary>
    [HttpGet("api/print/danh-sach-du-an")]
    public async Task<IActionResult> InDuAn([FromQuery] DuAnPrintSearchDto searchDto) {
        var fileNameTemplate = "DanhSachDuAn.xlsx";
        var templatePath = Path.Combine(
            AppContext.BaseDirectory,
            "ExportTemplates",
            fileNameTemplate
        );

        ManagedException.ThrowIf(!System.IO.File.Exists(templatePath), "Không tìm thấy file template");

        var data = await Mediator.Send(new DuAnGetDanhSachExportQuery(searchDto));

        var exportResult = _excelExporter.Export(new AsposeInstruction<DuAnExportDto> {
            TemplatePath = templatePath,
            Items = data,
            HiddenColumns = searchDto.HiddenColumns ?? [],
            AutoFitColumnsAndRows = false,
        });

        return new FileContentResult(exportResult.FileBytes, exportResult.ContentType) {
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

    #region DanhSachKhoKhanVuongMac

    /// <summary>
    /// DanhSachKhoKhanVuongMac.xlsx — Export danh sách khó khăn vướng mắc (filter giống danh-sach-tien-do)
    /// </summary>
    [HttpGet("api/print/danh-sach-kho-khan-vuong-mac")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> InKhoKhanVuongMac(
        [FromQuery] KhoKhanVuongMacPrintSearchModel searchModel,
        CancellationToken cancellationToken = default) {
        var fileNameTemplate = "DanhSachKhoKhanVuongMac.xlsx";
        var templatePath = Path.Combine(
            AppContext.BaseDirectory,
            "PrintTemplates",
            fileNameTemplate
        );

        ManagedException.ThrowIf(!System.IO.File.Exists(templatePath),
            "Không tìm thấy file template DanhSachKhoKhanVuongMac.xlsx");
        ManagedException.ThrowIf(_userProvider.Id == 0, "Vui lòng đăng nhập");

        var data = await Mediator.Send(new KhoKhanVuongMacGetDanhSachExportQuery {
            DuAnId = searchModel.DuAnId,
            BuocId = searchModel.BuocId,
            GlobalFilter = searchModel.GlobalFilter,
            NoiDung = searchModel.NoiDung,
            TinhTrangId = searchModel.TinhTrangId,
            MucDoKhoKhanId = searchModel.MucDoKhoKhanId,
            LoaiDuAnId = searchModel.LoaiDuAnId,
            LoaiDuAnTheoNamId = searchModel.LoaiDuAnTheoNamId,
            LanhDaoPhuTrachId = searchModel.LanhDaoPhuTrachId,
            DonViPhuTrachChinhId = searchModel.DonViPhuTrachChinhId,
            DonViPhoiHopId = searchModel.DonViPhoiHopId,
            TuNgay = searchModel.TuNgay,
            DenNgay = searchModel.DenNgay,
        }, cancellationToken);

        var exportResult = _excelExporter.Export(new AsposeInstruction<KhoKhanVuongMacExportDto> {
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

    #region DanhSachTongHopVanBanQuyetDinh

    /// <summary>
    /// DanhSachTongHopVanBanQuyetDinh.xlsx — Export tổng hợp văn bản quyết định (filter giống danh-sach-day-du)
    /// </summary>
    [HttpGet("api/print/danh-sach-tong-hop-van-ban-quyet-dinh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> InTongHopVanBanQuyetDinh(
        [FromQuery] TongHopVanBanQuyetDinhPrintSearchModel searchModel,
        CancellationToken cancellationToken = default) {
        var fileNameTemplate = "DanhSachTongHopVanBanQuyetDinh.xlsx";
        var templatePath = Path.Combine(
            AppContext.BaseDirectory,
            "PrintTemplates",
            fileNameTemplate
        );

        ManagedException.ThrowIf(!System.IO.File.Exists(templatePath),
            "Không tìm thấy file template DanhSachTongHopVanBanQuyetDinh.xlsx");
        ManagedException.ThrowIf(_userProvider.Id == 0, "Vui lòng đăng nhập");

        var data = await Mediator.Send(new TongHopVanBanQuyetDinhGetListExportQuery {
            DuAnId = searchModel.DuAnId,
            BuocId = searchModel.BuocId,
            GlobalFilter = searchModel.GlobalFilter,
            Loai = searchModel.Loai,
            TrichYeu = searchModel.TrichYeu,
            TuNgay = searchModel.TuNgay,
            DenNgay = searchModel.DenNgay,
            LoaiDuAnTheoNamId = searchModel.LoaiDuAnTheoNamId,
            CoQuanQuyetDinh = searchModel.CoQuanQuyetDinh,
        }, cancellationToken);

        var exportResult = _excelExporter.Export(new AsposeInstruction<TongHopVanBanQuyetDinhExportDto> {
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
            TenDuAn = searchModel.TenDuAn,
            DonViPhuTrachChinhId = searchModel.DonViPhuTrachChinhId,
            LoaiDuAnTheoNamId = searchModel.LoaiDuAnTheoNamId,
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
    /// TongHopNhuCauKinhPhiNam.xlsx — Export tổng hợp nhu cầu kinh phí năm
    /// </summary>
    [HttpGet("api/print/tong-hop-nhu-cau-kinh-phi-nam")]
    //[Authorize(Roles = RoleConstants.GroupTongHopNhuCauKinhPhiNamExport)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> InTongHopNhuCauKinhPhiNam(
        [FromQuery] DeXuatNhuCauKinhPhiNamPrintSearchModel searchModel,
        CancellationToken cancellationToken = default) {
        var fileNameTemplate = "TongHopNhuCauKinhPhiNam.xlsx";
        var templatePath = Path.Combine(
            AppContext.BaseDirectory,
            "PrintTemplates",
            fileNameTemplate
        );

        ManagedException.ThrowIf(!System.IO.File.Exists(templatePath), "Không tìm thấy file template");
        ManagedException.ThrowIf(_userProvider.Id == 0, "Vui lòng đăng nhập");

        var data = await Mediator.Send(new DeXuatNhuCauKinhPhiNamGetExportQuery {
            So = searchModel.So,
            TrichYeu = searchModel.TrichYeu,
            TuNgay = searchModel.TuNgay,
            DenNgay = searchModel.DenNgay,
            TrangThaiId = searchModel.TrangThaiId,
            GlobalFilter = searchModel.GlobalFilter,
        }, cancellationToken);

        var exportResult = _excelExporter.Export(new AsposeInstruction<TongHopNhuCauKinhPhiNamExportDto> {
            TemplatePath = templatePath,
            Items = data,
            HiddenColumns = searchModel.HiddenColumns ?? [],
            AutoFitColumnsAndRows = false,
        });

        return new FileContentResult(exportResult.FileBytes, exportResult.ContentType) {
            FileDownloadName = $"TongHopNhuCauKinhPhiNam_{DateTime.Now:yyyyMMddHHmmss}.xlsx"
        };
    }

    #endregion

    #region TinhHinhDeXuatNhuCau

    /// <summary>
    /// TinhHinhDeXuatNhuCau.xlsx — Export tổng hợp nhu cầu kinh phí năm
    /// </summary>
    [HttpGet("api/print/tinh-hinh-de-xuat-nhu-cau")]
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

        var exportResult = _excelExporter.Export(new AsposeInstruction<TinhHinhDeXuatNhuCauExportDto> {
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

    #region TinhHinhThucHienDauThau

    /// <summary>
    /// TinhHinhThucHienDauThau.xlsx — Export báo cáo tình hình thực hiện đấu thầu (Issue #103)
    /// </summary>
    [HttpGet("api/print/tinh-hinh-thuc-hien-dau-thau")]
    // [Authorize(Roles = RoleConstants.GroupTinhHinhThucHienDauThauExport)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> InTinhHinhThucHienDauThau(
        [FromQuery] TinhHinhThucHienDauThauPrintSearchDto searchDto,
        CancellationToken cancellationToken = default) {
        var fileNameTemplate = "TinhHinhThucHienDauThau.xlsx";
        var templatePath = Path.Combine(
            AppContext.BaseDirectory,
            "PrintTemplates",
            fileNameTemplate
        );

        ManagedException.ThrowIf(!System.IO.File.Exists(templatePath), "Không tìm thấy file template");
        ManagedException.ThrowIf(_userProvider.Id == 0, "Vui lòng đăng nhập");

        var result = await Mediator.Send(
            new GoiThauGetTinhHinhDauThauPrintQuery(searchDto),
            cancellationToken);

        var hiddenColumns = searchDto.HiddenColumns ?? [];

        AsposeResult exportResult;
        if (result.IsMultiSheet) {
            exportResult = _excelExporter.ExportDynamicMultiSheet(new DynamicMultiSheetInstruction {
                TemplatePath = templatePath,
                Sheets = result.Sheets.Select(sheet => new SheetInstruction {
                    Title = sheet.Title,
                    Items = ExporterHelper.ConvertToDictionaryList(sheet.Items),
                    HiddenColumns = hiddenColumns,
                }).ToList(),
            });
        } else {
            exportResult = _excelExporter.Export(new AsposeInstruction<TinhHinhThucHienDauThauExportDto> {
                TemplatePath = templatePath,
                Items = result.Items,
                HiddenColumns = hiddenColumns,
                AutoFitColumnsAndRows = false,
            });
        }

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

    #region DeXuatNhuCauKinhPhiChuTruong

    /// <summary>
    /// DeXuatNhuCauKinhPhiChuTruong.xlsx — Export đề xuất nhu cầu kinh phí chủ trương (mới / chuyển tiếp)
    /// </summary>
    [HttpGet("api/print/de-xuat-nhu-cau-kinh-phi-chu-truong")]
    [Authorize(Roles = RoleConstants.GroupDeXuatNhuCauKinhPhiChuTruongExport)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> InDeXuatNhuCauKinhPhiChuTruong(
        [FromQuery] TongHopDeXuatNhuCauKinhPhiPrintSearchModel searchModel,
        CancellationToken cancellationToken = default) {
        var fileNameTemplate = "DeXuatNhuCauKinhPhiChuTruong.xlsx";
        var templatePath = Path.Combine(
            AppContext.BaseDirectory,
            "PrintTemplates",
            fileNameTemplate
        );

        ManagedException.ThrowIf(!System.IO.File.Exists(templatePath), "Không tìm thấy file template");
        ManagedException.ThrowIf(_userProvider.Id == 0, "Vui lòng đăng nhập");

        var data = await Mediator.Send(new TongHopDeXuatNhuCauKinhPhiGetExportQuery {
            DuAnId = searchModel.DuAnId,
            BuocId = searchModel.BuocId,
            GlobalFilter = searchModel.GlobalFilter,
            Loai = searchModel.Loai,
            Nam = searchModel.Nam,
            LoaiDuAnTheoNamId = searchModel.LoaiDuAnTheoNamId,
            DonViPhuTrachId = searchModel.DonViPhuTrachId,
        }, cancellationToken);

        var exportResult = _excelExporter.Export(new AsposeInstruction<TongHopDeXuatNhuCauKinhPhiExportDto> {
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

    [HttpGet("api/print/bao-cao-tien-do-du-an")]
    public async Task<IActionResult> InBaoCaoTienDoDuAn([FromQuery] BaoCaoDuAnSearchDto searchModel) {
        var fileNameTemplate = "BaoCaoTienDoDuAn.xlsx";
        var procedureName = "usp_In_BaoCao_TienDo_DuAn";
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

        return new FileContentResult(exportResult.FileBytes, exportResult.ContentType) {
            FileDownloadName = GetDownloadFileName(fileNameTemplate)
        };
    }

    #region DanhSachBanGiaoHoSo

    /// <summary>
    /// DanhSachBanGiaoHoSo.xlsx — Export danh sách bàn giao hồ sơ (theo filter grid, không phân trang)
    /// </summary>
    [HttpGet("api/print/danh-sach-ban-giao-ho-so")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> InDanhSachBanGiaoHoSo(
        [FromQuery] BanGiaoHoSoSearchDto searchDto,
        CancellationToken cancellationToken = default) {
        var fileNameTemplate = "DanhSachBanGiaoHoSo.xlsx";
        var templatePath = Path.Combine(
            AppContext.BaseDirectory,
            "PrintTemplates",
            fileNameTemplate
        );

        ManagedException.ThrowIf(!System.IO.File.Exists(templatePath), "Không tìm thấy file template");

        var data = await Mediator.Send(new BanGiaoHoSoGetDanhSachExportQuery {
            SearchDto = searchDto
        }, cancellationToken);

        var exportResult = _excelExporter.Export(new AsposeInstruction<BanGiaoHoSoDanhSachExportDto> {
            TemplatePath = templatePath,
            Items = data,
            AutoFitColumnsAndRows = false,
        });

        return new FileContentResult(exportResult.FileBytes, exportResult.ContentType) {
            FileDownloadName = GetDownloadFileName(fileNameTemplate)
        };
    }

    #endregion

    #region DanhSachNoiDungDaKy

    /// <summary>
    /// DanhSachNoiDungDaKy.xlsx — Export danh sách nội dung đã ký (theo filter grid, không phân trang)
    /// </summary>
    [HttpGet("api/print/danh-sach-noi-dung-da-ky")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> InDanhSachNoiDungDaKy(
        [FromQuery] NoiDungDaKySearchDto searchDto,
        CancellationToken cancellationToken = default) {
        var fileNameTemplate = "DanhSachNoiDungDaKy.xlsx";
        var templatePath = Path.Combine(
            AppContext.BaseDirectory,
            "PrintTemplates",
            fileNameTemplate
        );

        ManagedException.ThrowIf(!System.IO.File.Exists(templatePath), "Không tìm thấy file template");

        var data = await Mediator.Send(
            new NoiDungDaKyGetDanhSachExportQuery(searchDto),
            cancellationToken);

        var exportResult = _excelExporter.Export(new AsposeInstruction<NoiDungDaKyExportDto> {
            TemplatePath = templatePath,
            Items = data,
            HiddenColumns = searchDto.HiddenColumns ?? [],
            AutoFitColumnsAndRows = false,
        });

        return new FileContentResult(exportResult.FileBytes, exportResult.ContentType) {
            FileDownloadName = GetDownloadFileName(fileNameTemplate)
        };
    }

    #endregion

    #region DanhSachQuanLyPheDuyet

    /// <summary>
    /// DanhSachQuanLyPheDuyet.xlsx — Export danh sách phê duyệt (theo filter type/trangThai, không phân trang)
    /// </summary>
    [HttpGet("api/print/danh-sach-quan-ly-phe-duyet")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> InDanhSachQuanLyPheDuyet(
        [FromQuery] string? type,
        [FromQuery] string? trangThai,
        CancellationToken cancellationToken = default) {
        var fileNameTemplate = "DanhSachQuanLyPheDuyet.xlsx";
        var templatePath = Path.Combine(
            AppContext.BaseDirectory,
            "PrintTemplates",
            fileNameTemplate
        );

        ManagedException.ThrowIf(!System.IO.File.Exists(templatePath), "Không tìm thấy file template");

        // Cùng nguồn với GET /api/phe-duyet/danh-sach — PageSize=0 = lấy hết
        var list = await Mediator.Send(new PheDuyetGetDanhSachQuery {
            Type = type,
            TrangThai = trangThai,
            PageIndex = 1,
            PageSize = 0,
            IncludeAttachments = false,
        }, cancellationToken);

        ManagedException.ThrowIf(list.Data.Count == 0, "Không có dữ liệu để xuất");

        var data = PheDuyetExportMappings.ToExportDtos(list.Data);

        var exportResult = _excelExporter.Export(new AsposeInstruction<PheDuyetExportDto> {
            TemplatePath = templatePath,
            Items = data,
            AutoFitColumnsAndRows = false,
        });

        return new FileContentResult(exportResult.FileBytes, exportResult.ContentType) {
            FileDownloadName = GetDownloadFileName(fileNameTemplate)
        };
    }

    #endregion

    #region FileBanGiaoHoSo

    /// <summary>
    /// DanhSachFileBanGiaoHoSo.xlsx — Export danh sách tệp HS bàn giao của một bản ghi
    /// </summary>
    [HttpGet("api/print/file-ban-giao-ho-so")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> InFileBanGiaoHoSo(
        [FromQuery] Guid id,
        CancellationToken cancellationToken = default) {
        var fileNameTemplate = "DanhSachFileBanGiaoHoSo.xlsx";
        var templatePath = Path.Combine(
            AppContext.BaseDirectory,
            "PrintTemplates",
            fileNameTemplate
        );

        ManagedException.ThrowIf(!System.IO.File.Exists(templatePath), "Không tìm thấy file template");

        var result = await Mediator.Send(new BanGiaoHoSoGetFileExportQuery(id), cancellationToken);

        var rows = new List<Dictionary<string, object?>>();
        for (var i = 0; i < result.Files.Count; i++) {
            var file = result.Files[i];
            rows.Add(new Dictionary<string, object?> {
                ["Level"] = i == 0 ? 1 : 2,
                ["TenDuAn"] = i == 0 ? result.TenDuAn : null,
                ["TenFile"] = file.TenFile,
                ["ThoiGianDinhKem"] = file.ThoiGianDinhKem.LocalDateTime.ToString("dd/MM/yyyy"),
            });
        }

        var exportResult = _excelExporter.ExportMultiLevelHierarchical(new MultiLevelHierarchicalInstruction {
            TemplatePath = templatePath,
            Rows = rows,
            RootLevel = 1,
            MergedColumnIndices = [0, 1],
            SttPropertyName = "Stt",
            SttColumnIndex = 0,
        });

        return new FileContentResult(exportResult.FileBytes, exportResult.ContentType) {
            FileDownloadName = GetDownloadFileName(fileNameTemplate)
        };
    }

    #endregion

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

    #region Xuất tờ trình kế hoạch lcnt
    [HttpGet("api/print/phieu-trinh-phe-duyet")]
    [ProducesResponseType<ResultApi<FileContentResult>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> InPhieuTrinhPheDuyet([FromQuery] Guid id) {
        try {


            var fileNameTemplate = "PhieuTrinhPheDuyet.docx";
            var templatePath = Path.Combine(
                AppContext.BaseDirectory,
                "PrintTemplates",
                "Word",
                fileNameTemplate
            );

            ManagedException.ThrowIf(!System.IO.File.Exists(templatePath), "Không tìm thấy file template PhieuTrinhPheDuyet.docx");

            var entity = await Mediator.Send(new ToTrinhPheDuyetGetExportQuery() {
                Id = id,
                ThrowIfNull = true,
                IsNoTracking = true
            });

            var doc = new Aspose.Words.Document(templatePath);
            doc.MailMerge.UseNonMergeFields = true;
            DateTime? ngayToTrinh = entity.NgayToTrinh?.ToOffset(TimeSpan.FromHours(7)).Date;


            var replacements = new Dictionary<string, string> {
       { "ngay", entity.NgayToTrinh.HasValue
           ? $"ngày {ngayToTrinh.Value:dd} tháng {ngayToTrinh.Value:MM} năm {ngayToTrinh.Value:yyyy}"
           : $"ngày  tháng  năm " },

       { "Ten", entity.Ten ?? "" },
       { "So", entity.So ?? "" },

       { "NguoiDuyet" , entity.TenLanhDaoPhuTrach != null ? entity.TenLanhDaoPhuTrach : "" },
       { "DonViTrinh" , entity.TenDonViTrinh != null ? entity.TenDonViTrinh: "" },
       { "TenDuAn", entity.TenDuAn ?? "" },
       { "NgayToTrinh", (ngayToTrinh??DateTime.Now).ToString("dd/MM/yyyy")},
       { "TrichYeu", entity.TrichYeu ?? "" }
   };


            var bytes = _wordHelper.ExportFromTemplate(templatePath, replacements);

            return File(bytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                GetDownloadFileName(fileNameTemplate));

        } catch (Exception ex) {
            Log.Error("in phe duyet" + ex.Message);
            throw;
        }
    }
    #endregion

    #region KeHoachTrienKhaiHangMuc

    /// <summary>
    /// KeHoachTrienKhaiHangMuc.xlsx — Export kế hoạch triển khai hạng mục theo giai đoạn (PMIS #9469)
    /// </summary>
    [HttpGet("api/print/ke-hoach-trien-khai-hang-muc")]
    [Authorize(Roles = RoleConstants.GroupKeHoachTrienKhaiHangMucExport)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> InKeHoachTrienKhaiHangMuc(
        [FromQuery] KeHoachTrienKhaiHangMucPrintSearchDto searchDto,
        CancellationToken cancellationToken = default) {
        var fileNameTemplate = "KeHoachTrienKhaiHangMuc.xlsx";
        var templatePath = Path.Combine(
            AppContext.BaseDirectory,
            "ExportTemplates",
            fileNameTemplate
        );

        ManagedException.ThrowIf(!System.IO.File.Exists(templatePath), "Không tìm thấy file template");
        ManagedException.ThrowIf(_userProvider.Id == 0, "Vui lòng đăng nhập");

        var rows = await Mediator.Send(new KeHoachTrienKhaiHangMucGetExportQuery {
            Id = searchDto.Id,
            DuAnId = searchDto.DuAnId,
            BuocId = searchDto.BuocId,
            So = searchDto.So,
            TrichYeu = searchDto.TrichYeu,
            TuNgay = searchDto.TuNgay,
            DenNgay = searchDto.DenNgay,
            GlobalFilter = searchDto.GlobalFilter,
            TrangThaiId = searchDto.TrangThaiId,
            LoaiDuAnTheoNamId = searchDto.LoaiDuAnTheoNamId,
        }, cancellationToken);

        var exportResult = _excelExporter.Export(new AsposeInstruction<KeHoachTrienKhaiHangMucExportItemDto> {
            TemplatePath = templatePath,
            Items = rows,
            HiddenColumns = searchDto.HiddenColumns ?? [],
            AutoFitColumnsAndRows = false,
        });

        exportResult = KeHoachTrienKhaiHangMucExportStyler.Apply(
            exportResult, rows, _asposeHelper, templatePath);

        return new FileContentResult(exportResult.FileBytes, exportResult.ContentType) {
            FileDownloadName = $"KeHoachTrienKhaiHangMuc_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
        };
    }

    /// <summary>
    /// PhieuTrinhKeHoachTrienKhaiHangMuc.docx — Xuất phiếu trình kế hoạch triển khai hạng mục (PMIS #9469)
    /// </summary>
    [HttpGet("api/print/phieu-trinh-ke-hoach-trien-khai-hang-muc")]
    [Authorize(Roles = RoleConstants.GroupKeHoachTrienKhaiHangMucExport)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> InPhieuTrinhKeHoachTrienKhaiHangMuc(
        [FromQuery] Guid id,
        CancellationToken cancellationToken = default) {
        var fileNameTemplate = "PhieuTrinhKeHoachTrienKhaiHangMuc.docx";
        var templatePath = Path.Combine(
            AppContext.BaseDirectory,
            "PrintTemplates",
            "Word",
            fileNameTemplate);

        ManagedException.ThrowIf(!System.IO.File.Exists(templatePath), "Không tìm thấy file template");

        var dto = await Mediator.Send(
            new KeHoachTrienKhaiHangMucGetPhieuTrinhPrintQuery { Id = id },
            cancellationToken);

        var bytes = _keHoachWordExporter.Export(templatePath, dto);

        return File(bytes,
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            GetDownloadFileName(fileNameTemplate));
    }

    #endregion


    #region  Xuất tờ trình phân khai kinh phí
    [HttpGet("api/print/phieu-trinh-phan-khai-kinh-phi")]
    [ProducesResponseType<ResultApi<FileContentResult>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> InPhieuTrinhPhanKhaiKinhPhi([FromQuery] Guid id, CancellationToken cancellationToken = default) {
        try {
            var fileNameTemplate = "ToTrinhPhanKhaiKinhPhi.docx";
            var templatePath = Path.Combine(
                AppContext.BaseDirectory,
                "PrintTemplates",
                "Word",
                fileNameTemplate
            );

            ManagedException.ThrowIf(!System.IO.File.Exists(templatePath), "Không tìm thấy file template ToTrinhPhanKhaiKinhPhi.docx");

            var data = await Mediator.Send(new PhanKhaiKinhPhiGetDanhSachExportQuery {
                Id = id,

            }, cancellationToken);
            var entity = data != null && data.Count > 0 ? data[0] : null;
            var doc = new Aspose.Words.Document(templatePath);
            doc.MailMerge.UseNonMergeFields = true;
            DateTime? ngayToTrinh = data[0].NgayToTrinh?.ToOffset(TimeSpan.FromHours(7)).Date;
            var culture = new CultureInfo("vi-VN");
            var replacements = new Dictionary<string, string> {
                { "ngay", entity.NgayToTrinh.HasValue
                    ? $"ngày {ngayToTrinh.Value:dd} tháng {ngayToTrinh.Value:MM} năm {ngayToTrinh.Value:yyyy}"
                    : $"ngày  tháng  năm " },


                { "So", entity.SoToTrinh ?? "" },

                { "TenDuAn", entity.TenDuAn ?? "" },
                { "KinhPhiPhanKhai", entity.KinhPhiPhanKhai?.ToString("N0", culture) ?? "0"},
                { "TongMucDauTu", entity.TongMucDauTu?.ToString("N0", culture) ?? "0"},
                { "NgayToTrinh", (ngayToTrinh??DateTime.Now).ToString("dd/MM/yyyy")},
                { "NamToTrinh", (ngayToTrinh??DateTime.Now).ToString("yyyy")},
              //  { "TrichYeu", entity.TrichYeu ?? "" }
            };


            var bytes = _wordHelper.ExportFromTemplate(templatePath, replacements);

            return File(bytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                GetDownloadFileName(fileNameTemplate));

        } catch (Exception ex) {
            Log.Error("in phe duyet" + ex.Message);
            throw;
        }
    }
    [HttpGet("api/print/phieu-trinh-giao-nhiem-vu-phan-khai-kinh-phi")]
    [ProducesResponseType<ResultApi<FileContentResult>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> InPhieuTrinhGiaoNhiemVuPhanKhai([FromQuery] Guid id, CancellationToken cancellationToken = default) {
        try {
            var fileNameTemplate = "PhieuTrinhGiaoNhiemVu.docx";
            var templatePath = Path.Combine(
                AppContext.BaseDirectory,
                "PrintTemplates",
                "Word",
                fileNameTemplate
            );

            ManagedException.ThrowIf(!System.IO.File.Exists(templatePath), "Không tìm thấy file template PhieuTrinhGiaoNhiemVu.docx");

            var entity = await Mediator.Send(new PhanKhaiKinhPhiGetQuery {
                Id = id,
                IsNoTracking = false

            }, cancellationToken);
            var doc = new Aspose.Words.Document(templatePath);
            doc.MailMerge.UseNonMergeFields = true;
            DateTime? ngayToTrinh = entity.NgayToTrinh?.ToOffset(TimeSpan.FromHours(7)).Date;
            var culture = new CultureInfo("vi-VN");
            var replacements = new Dictionary<string, string> {
            { "ngay", entity.NgayToTrinh.HasValue
                ? $"ngày {ngayToTrinh.Value:dd} tháng {ngayToTrinh.Value:MM} năm {ngayToTrinh.Value:yyyy}"
                : $"ngày  tháng  năm " },


            { "So", entity.SoToTrinh ?? "" },
            { "TenDuAn", entity.DuAn?.TenDuAn ?? "" },
            { "KinhPhiPhanKhai", entity.KinhPhiPhanKhai?.ToString("N0", culture) ?? "0"},
            { "TenNguonVon", entity.NguonVon?.Ten ?? ""},
            { "KinhPhiDeXuat", entity.KinhPhiDeXuat?.ToString("N0", culture) ?? "0"},
            { "TongMucDauTu", entity.DuAn?.TongMucDauTu?.ToString("N0", culture) ?? "0"},
            { "NgayToTrinh", (ngayToTrinh??DateTime.Now).ToString("dd/MM/yyyy")},
            { "TrichYeu", entity.TrichYeu ?? "" },
            { "ThuyetMinh", entity.ThuyetMinh ?? "" }
        };


            var bytes = _wordHelper.ExportFromTemplate(templatePath, replacements);

            return File(bytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                GetDownloadFileName(fileNameTemplate));

        } catch (Exception ex) {
            Log.Error("in phe duyet" + ex.Message);
            throw;
        }
    }

    #endregion

    #region Xuat To Trinh Ke Hoach Lua Chon Nha Thau  iss 9471

    [HttpGet("api/print/trien-khai-ke-hoach-lua-chon-nha-thau")]
    [Authorize(Roles = RoleConstants.GroupKeHoachTrienKhaiHangMucExport)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> InTrienKhaiKeHoachLuaChonNhaThau(
        [FromQuery] Guid id,
        CancellationToken cancellationToken = default) {
        var fileNameTemplate = "ToTrinhTrienKhaiKeHoachLuaChonNhaThau.docx";
        var templatePath = Path.Combine(
             AppContext.BaseDirectory,
             "PrintTemplates",
             "Word",
             fileNameTemplate
         );

        ManagedException.ThrowIf(!System.IO.File.Exists(templatePath), "Không tìm thấy file template");
        ManagedException.ThrowIf(_userProvider.Id == 0, "Vui lòng đăng nhập");

        var rows = await Mediator.Send(new TrienKhaiKeHoachLCNTGetQuery {
            Id = id,

        }, cancellationToken);

        var doc = new Aspose.Words.Document(templatePath);
        doc.MailMerge.UseNonMergeFields = true;
        DateTime? ngayToTrinh = rows.NgayTrinh.ToOffset(TimeSpan.FromHours(7)).Date;
        var culture = new CultureInfo("vi-VN");
        var replacements = new Dictionary<string, string> {
            { "TenGoiThau", rows?.GoiThau?.Ten},
            { "NgayTrinh", (ngayToTrinh??DateTime.Now).ToString("dd/MM/yyyy")},
            { "So", rows.So},
            { "GiaTri", rows.GiaTri?.ToString("N0", culture) ?? "0" },
            { "NoiDung", rows?.NoiDung },
            { "YeuCau", rows?.YeuCau },
            { "ThoiGianThucHien", rows?.ThoiGianThucHien },
            { "TenHinhThucLCNT", rows?.DmHinhThucLCNT?.Ten??"" },

        };


        var bytes = _wordHelper.ExportFromTemplate(templatePath, replacements);

        return File(bytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            GetDownloadFileName(fileNameTemplate));


    }

    #endregion

    #region Xuất tờ trình thành lập ban qlda
    [HttpGet("api/print/to-trinh-lap-ban-qlda")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> InToTrinhThanhLapBanQLDA(
       [FromQuery] Guid id, [FromQuery] bool isMauDuThao ,
       CancellationToken cancellationToken = default) {
        var fileNameTemplate = isMauDuThao? "MauDuTaoThanhLapBanQLDA.docx" : "ToTrinhThanhLapBanQLDA.docx";
        var templatePath = Path.Combine(
             AppContext.BaseDirectory,
             "PrintTemplates",
             "Word",
             fileNameTemplate
         );

        ManagedException.ThrowIf(!System.IO.File.Exists(templatePath), "Không tìm thấy file template");

        var rows = await Mediator.Send(new QuyetDinhLapBanQldaGetQuery {
            Id = id,
            IncludeThanhVien= true,
        }, cancellationToken);

        var doc = new Aspose.Words.Document(templatePath);
        doc.MailMerge.UseNonMergeFields = true;
        //   DateTime? ngayToTrinh = rows.NgayTrinh.ToOffset(TimeSpan.FromHours(7)).Date;
        DateTime ngayHienTai = DateTime.Now;
        var replacements = new Dictionary<string, string> {
            { "So", rows.So},
            { "TrichYeu", rows?.TrichYeu??"" },
            { "SoDuThao", rows?.SoDuThao??rows.So},
            { "TrichYeuDuThao", rows?.TrichYeuDuThao??rows?.TrichYeu},
         //   { "ngayToTrinh", (ngayToTrinh??DateTime.Now).ToString("dd/MM/yyyy")},
            { "NgayThangNam", $"Ngày {ngayHienTai.ToString("dd")} tháng {ngayHienTai.ToString("MM") } năm {ngayHienTai.Year}"},
            { "TenLanhDaoPhuTrach", ""}// rows.DuAn?.LanhDaoPhuTrachId

        };
       
        DataTable dt = rows.ThanhViens?.ToDataTable()??
               DataTableConvertExtensions.CreateDataTable<ThanhVienBanQLDA>("ThanhVien");
        dt.TableName = "ThanhVien";
        DataSet ds = new DataSet();
        ds.Tables.Add(dt);
        var bytes = _wordHelper.ExportFromTemplate(templatePath, ds, replacements);

        return File(bytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            GetDownloadFileName(fileNameTemplate));


    }

    #endregion

}

/// <summary>
/// Print-only projection of <see cref="DuAnBuocStateDto"/>:
///   - Remaps Level &lt;= 0 → Level = 1 để tránh Stack empty pop trong <c>ExportWithOutline</c>
///     (algorithm dùng Count &gt;= level; với root MaterializedPathEntity rows = 0 → throw).
///   - Convert 4 cột ngày thành string dd/MM/yyyy (UTC) để file in chỉ hiển thị ngày, không có giờ.
///   - Giữ nguyên tên field để khớp với placeholder $NgayDuKienBatDau ... trong template QuyTrinhTrinhDuAn.xlsx.
/// </summary>
public class DuAnBuocStatePrintRow {
    public int Id { get; set; }
    public string TenDuAn { get; set; } = string.Empty;
    public int QuyTrinhId { get; set; }
    public string TenQuyTrinh { get; set; } = string.Empty;
    public int? GiaiDoanId { get; set; }
    public string? TenGiaiDoan { get; set; }
    public int? BuocId { get; set; }
    public string TenBuoc { get; set; } = string.Empty;
    public string? PartialView { get; set; }
    public int? ParentId { get; set; }
    public string? Path { get; set; }
    public int Level { get; set; }
    public int Stt { get; set; }
    public string? HierarchicalStt { get; set; }
    public int? TrangThaiId { get; set; }
    public string? NgayDuKienBatDau { get; set; }
    public string? NgayDuKienKetThuc { get; set; }
    public string? NgayThucTeBatDau { get; set; }
    public string? NgayThucTeKetThuc { get; set; }
    public string? GhiChu { get; set; }
    public string? TrachNhiemThucHien { get; set; }
    public bool IsKetThuc { get; set; }
    public long? PhongPhuTrachChinhId { get; set; }
    public string? PhongBanPhuTrachChinh { get; set; }
    public List<string> DanhSachPhongBanPhoiHops { get; set; } = [];
}
