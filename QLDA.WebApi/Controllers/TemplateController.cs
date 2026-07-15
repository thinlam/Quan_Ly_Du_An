using Microsoft.EntityFrameworkCore;
using QLDA.Application.PhanKhaiKinhPhis;
using QLDA.Application.KeHoachTrienKhaiHangMucs.Queries;
using QLDA.Domain.Constants;

namespace QLDA.WebApi.Controllers;

[Tags("Mẫu")]
[Route("api/template")]
public class TemplateController(IServiceProvider serviceProvider) : AggregateRootController(serviceProvider) {
    private readonly IImporterHelper _excelImporter = serviceProvider.GetRequiredService<IImporterHelper>();
    private readonly IRepository<DuAn, Guid> DuAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();

    private readonly IRepository<DuAnBuoc, int> DuAnBuoc =
        serviceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();

    private readonly IRepository<DanhMucLoaiHopDong, int> LoaiHopDong =
        serviceProvider.GetRequiredService<IRepository<DanhMucLoaiHopDong, int>>();

    private readonly IRepository<DanhMucHinhThucLuaChonNhaThau, int> HinhThucLuaChonNhaThau =
        serviceProvider.GetRequiredService<IRepository<DanhMucHinhThucLuaChonNhaThau, int>>();

    private readonly IRepository<DanhMucPhuongThucLuaChonNhaThau, int> PhuongThucLuaChonNhaThau =
        serviceProvider.GetRequiredService<IRepository<DanhMucPhuongThucLuaChonNhaThau, int>>();

    private readonly IRepository<DanhMucNguonVon, int> NguonVon =
        serviceProvider.GetRequiredService<IRepository<DanhMucNguonVon, int>>();

    private readonly IRepository<DanhMucTrangThaiDuAn, int> TrangThaiDuAn =
        serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiDuAn, int>>();

    private readonly IRepository<KeHoachLuaChonNhaThau, Guid> KeHoachLuaChonNhaThau =
        serviceProvider.GetRequiredService<IRepository<KeHoachLuaChonNhaThau, Guid>>();

    [HttpGet("import-bao-cao-tien-do")]

    [ProducesResponseType<FileContentResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<FileContentResult> GetBaoCaoTienDo([FromQuery] Guid? duAnId = null) {
        var fileNameTemplate = "Import_BaoCaoTienDo.xlsx";
        var templatePath = Path.Combine(
            AppContext.BaseDirectory, // ví dụ: ...\QLDA.WebApi
            "PrintTemplates", // chính xác tên folder trong project
            fileNameTemplate
        );

        var duAnQuery = DuAn.GetQueryableSet().Where(e => !e.IsDeleted);
        if (duAnId.HasValue) duAnQuery = duAnQuery.Where(e => e.Id == duAnId.Value);

        var danhSachTenDuAn = await duAnQuery
            .Select(e => new ComboData {
                Name = e.TenDuAn ?? string.Empty,
                Id = e.Id.ToString(),
            }).ToListAsync();

        var danhSachTenBuoc = await DuAnBuoc.GetQueryableSet().Where(e => !e.IsDeleted)
            .WhereIf(duAnId.HasValue, e => e.DuAnId == duAnId!.Value)
            .Select(e => new ComboData() {
                Id = e.Id.ToString(),
                ParentId = e.DuAnId.ToString(),
                Name = e.TenBuoc ?? e.Buoc!.Ten ?? string.Empty
            }).Distinct().ToListAsync();
        List<List<ComboData>> comboData = [
            danhSachTenDuAn,
            danhSachTenBuoc
        ];

        var importResult = _excelImporter.GetTemplate(templatePath, comboData);

        return new FileContentResult(importResult.FileBytes,
            importResult.ContentType) {
            FileDownloadName = fileNameTemplate
        };
    }

    [HttpGet("import-goi-thau")]

    [ProducesResponseType<FileContentResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<FileContentResult> GetImportGoiThau([FromQuery] Guid? duAnId = null) {
        var fileNameTemplate = "Import_GoiThau.xlsx";
        var templatePath = Path.Combine(
            AppContext.BaseDirectory,
            "PrintTemplates",
            fileNameTemplate
        );

        var danhSachLoaiHopDong = await LoaiHopDong.GetQueryableSet().Where(e => !e.IsDeleted)
            .Select(e => new ComboData {
                Name = e.Ten ?? string.Empty,
                Id = e.Id.ToString(),
            }).ToListAsync();

        var danhSachHinhThuc = await HinhThucLuaChonNhaThau.GetQueryableSet().Where(e => !e.IsDeleted)
            .Select(e => new ComboData {
                Name = e.Ten ?? string.Empty,
                Id = e.Id.ToString(),
            }).ToListAsync();

        var danhSachPhuongThuc = await PhuongThucLuaChonNhaThau.GetQueryableSet().Where(e => !e.IsDeleted)
            .Select(e => new ComboData {
                Name = e.Ten ?? string.Empty,
                Id = e.Id.ToString(),
            }).ToListAsync();

        var danhSachNguonVon = await NguonVon.GetQueryableSet().Where(e => !e.IsDeleted)
            .Select(e => new ComboData {
                Name = e.Ten ?? string.Empty,
                Id = e.Id.ToString(),
            }).ToListAsync();

        var danhSachKeHoach = await KeHoachLuaChonNhaThau.GetQueryableSet().Where(e => !e.IsDeleted)
            .WhereIf(duAnId.HasValue, e => e.DuAnId == duAnId!.Value)
            .Select(e => new ComboData {
                Name = e.Ten ?? string.Empty,
                Id = e.Id.ToString(),
            }).ToListAsync();

        List<List<ComboData>> comboData = [
            danhSachKeHoach,
            danhSachNguonVon,
            danhSachHinhThuc,
            danhSachPhuongThuc,
            danhSachLoaiHopDong,
        ];

        var importResult = _excelImporter.GetTemplate(templatePath, comboData);

        return new FileContentResult(importResult.FileBytes,
            importResult.ContentType) {
            FileDownloadName = fileNameTemplate
        };
    }

    [HttpGet("import-de-xuat-chu-truong-chuyen-tiep")]
    [ProducesResponseType<FileContentResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<FileContentResult> GetImportDeXuatChuTruongChuyenTiep([FromQuery] Guid? duAnId) {
        var fileNameTemplate = "Import_DeXuatChuTruongChuyenTiep.xlsx";
        var templatePath = Path.Combine(
            AppContext.BaseDirectory,
            "PrintTemplates",
            fileNameTemplate
        );

        var duAnQuery = DuAn.GetQueryableSet().Where(e => !e.IsDeleted);

        if (duAnId.HasValue)
            duAnQuery = duAnQuery.Where(e => e.Id == duAnId.Value);
        else
            duAnQuery = duAnQuery.Where(e => e.TrangThaiDuAn!.Ma == "DTH");

        var danhSachTenDuAn = await duAnQuery
            .Select(e => new ComboData {
                Name = e.TenDuAn ?? string.Empty,
                Id = e.Id.ToString(),
            }).ToListAsync();

        List<List<ComboData>> comboData = [danhSachTenDuAn];

        var importResult = _excelImporter.GetTemplate(templatePath, comboData);

        return new FileContentResult(importResult.FileBytes,
            importResult.ContentType) {
            FileDownloadName = fileNameTemplate
        };
    }

    [HttpGet("import-phan-khai-kinh-phi")]
    [ProducesResponseType<FileContentResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<FileContentResult> GetImportPhanKhaiKinhPhi(
        [FromQuery] Guid? duAnId = null,
        CancellationToken cancellationToken = default) {
        var fileNameTemplate = "Import_PhanKhaiKinhPhi.xlsx";
        var templatePath = Path.Combine(
            AppContext.BaseDirectory,
            "PrintTemplates",
            fileNameTemplate
        );

        var duAnQuery = DuAn.GetQueryableSet().Where(e => !e.IsDeleted);
        if (duAnId.HasValue)
        {
            duAnQuery = duAnQuery.Where(e => e.Id == duAnId.Value);
        }
        else
        {
            var trangThaiHoanThanh = await TrangThaiDuAn
                .GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
                .FirstOrDefaultAsync(
                    s => s.Ma == DanhMucTrangThaiDuAnCodes.HoanThanh,
                    cancellationToken);

            if (trangThaiHoanThanh != null)
            {
                duAnQuery = duAnQuery.Where(e => e.TrangThaiDuAnId != trangThaiHoanThanh.Id);
            }
        }

        var danhSachDuAn = await duAnQuery
            .Select(e => new ComboData {
                Name = e.TenDuAn ?? string.Empty,
                Id = e.Id.ToString(),
            }).ToListAsync(cancellationToken);

        var duAns = await duAnQuery
            .Include(e => e.DuAnNguonVons!)
            .ThenInclude(dnv => dnv.NguonVon)
            .ToListAsync(cancellationToken);

        var nguonVonItems = duAns
            .SelectMany(e => (e.DuAnNguonVons ?? [])
                .Where(dnv => dnv.NguonVon != null)
                .Select(dnv => new {
                    TenDuAn = e.TenDuAn ?? string.Empty,
                    TenNguonVon = dnv.NguonVon!.Ten ?? string.Empty,
                    NguonVonId = dnv.RightId,
                }))
            .Distinct()
            .OrderBy(x => x.TenDuAn)
            .ThenBy(x => x.TenNguonVon)
            .ToList();

        var danhSachNguonVon = nguonVonItems
            .Select(x => new ComboData {
                Name = PhanKhaiKinhPhiImportDisplay.Format(x.TenNguonVon, x.TenDuAn),
                Id = x.NguonVonId.ToString(),
            })
            .ToList();

        List<List<ComboData>> comboData = [danhSachDuAn, danhSachNguonVon];

        var importResult = _excelImporter.GetTemplate(templatePath, comboData);

        return new FileContentResult(importResult.FileBytes,
            importResult.ContentType) {
            FileDownloadName = fileNameTemplate
        };
    }

    [HttpGet("import-ke-hoach-trien-khai-hang-muc")]
    [ProducesResponseType<FileContentResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<FileContentResult> GetImportKeHoachTrienKhaiHangMuc(
        [FromQuery] Guid? duAnId = null,
        CancellationToken cancellationToken = default) {
        const string fileNameTemplate = "Import_KeHoachTrienKhaiHangMuc.xlsx";
        var templatePath = Path.Combine(
            AppContext.BaseDirectory,
            "PrintTemplates",
            fileNameTemplate);

        var comboData = await Mediator.Send(
            new KeHoachTrienKhaiHangMucGetImportTemplateQuery(duAnId),
            cancellationToken);

        var importResult = _excelImporter.GetTemplate(
            templatePath,
            comboData,
            multiValueComboIndices: new HashSet<int> { 5, 6 });

        return new FileContentResult(importResult.FileBytes, importResult.ContentType) {
            FileDownloadName = fileNameTemplate,
        };
    }
}