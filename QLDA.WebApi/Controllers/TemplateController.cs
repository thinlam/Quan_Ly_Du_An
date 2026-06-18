using Microsoft.EntityFrameworkCore;
using QLDA.Domain.Entities.DanhMuc;

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
}