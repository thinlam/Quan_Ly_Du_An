using Microsoft.EntityFrameworkCore;
using QLDA.Application.GoiThaus.DTOs;
using QLDA.Domain.Enums;

namespace QLDA.Application.GoiThaus.Queries;

public record GoiThauGetTinhHinhDauThauPrintQuery(
    TinhHinhThucHienDauThauPrintSearchDto SearchDto)
    : IRequest<TinhHinhThucHienDauThauPrintResultDto>;

internal class GoiThauGetTinhHinhDauThauPrintQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<GoiThauGetTinhHinhDauThauPrintQuery, TinhHinhThucHienDauThauPrintResultDto>
{
    private static readonly (TinhHinhThucHienDauThauLoai Loai, string Title)[] SheetTabs =
    [
        (TinhHinhThucHienDauThauLoai.ChuaCoKetQua, "Chưa có kết quả"),
        (TinhHinhThucHienDauThauLoai.CoKetQua, "Có kết quả"),
        (TinhHinhThucHienDauThauLoai.DaLenHopDong, "Đã lên hợp đồng"),
    ];

    private readonly IRepository<GoiThau, Guid> _goiThau =
        serviceProvider.GetRequiredService<IRepository<GoiThau, Guid>>();
    private readonly IRepository<DuAnBuoc, int> _duAnBuoc =
        serviceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();

    public async Task<TinhHinhThucHienDauThauPrintResultDto> Handle(
        GoiThauGetTinhHinhDauThauPrintQuery request,
        CancellationToken cancellationToken = default)
    {
        var loaiValue = request.SearchDto.Loai ?? 0;

        ManagedException.ThrowIf(
            !Enum.IsDefined(typeof(TinhHinhThucHienDauThauLoai), loaiValue),
            "Loại tab không hợp lệ. Chỉ chấp nhận giá trị 1 (Chưa có kết quả), 2 (Có kết quả), 3 (Đã lên hợp đồng), hoặc bỏ trống để xuất cả 3 tab.");

        var loai = (TinhHinhThucHienDauThauLoai)loaiValue;

        if (loai is TinhHinhThucHienDauThauLoai.TatCa)
        {
            var sheets = new List<TinhHinhThucHienDauThauSheetDto>(SheetTabs.Length);
            foreach (var tab in SheetTabs)
            {
                var items = await GetExportItemsAsync(tab.Loai, request.SearchDto, cancellationToken);
                sheets.Add(new TinhHinhThucHienDauThauSheetDto
                {
                    Title = tab.Title,
                    Items = items,
                });
            }

            return new TinhHinhThucHienDauThauPrintResultDto
            {
                IsMultiSheet = true,
                Sheets = sheets,
            };
        }

        var data = await GetExportItemsAsync(loai, request.SearchDto, cancellationToken);

        return new TinhHinhThucHienDauThauPrintResultDto
        {
            IsMultiSheet = false,
            Items = data,
        };
    }

    private async Task<List<TinhHinhThucHienDauThauExportDto>> GetExportItemsAsync(
        TinhHinhThucHienDauThauLoai loai,
        TinhHinhThucHienDauThauPrintSearchDto searchDto,
        CancellationToken cancellationToken)
    {
        var duAnBuocQuery = _duAnBuoc.GetQueryableSet().AsNoTracking();

        var queryable = _goiThau.GetOrderedSet()
            .Include(e => e.KetQuaTrungThau)
            .Include(e => e.HopDong)
            .AsQueryable()
            .ApplyTinhHinhDauThauFilters(searchDto.DuAnId, searchDto.GiaiDoanId)
            .ApplyTinhHinhDauThauNamDuAnFilter(searchDto.NamDuAn);

        queryable = loai switch
        {
            TinhHinhThucHienDauThauLoai.ChuaCoKetQua => queryable.Where(e => e.KetQuaTrungThau == null && e.HopDong == null),
            TinhHinhThucHienDauThauLoai.CoKetQua => queryable.Where(e => e.KetQuaTrungThau != null && e.HopDong == null),
            TinhHinhThucHienDauThauLoai.DaLenHopDong => queryable.Where(e => e.KetQuaTrungThau != null && e.HopDong != null),
            _ => queryable.Where(_ => false),
        };

        var rows = await queryable
            .AsNoTracking()
            .Select(e => new
            {
                TenDuAn = e.DuAn != null ? e.DuAn.TenDuAn : null,
                TenBuoc = e.DuAnBuoc != null
                    ? (e.DuAnBuoc.TenBuoc ?? (e.DuAnBuoc.Buoc != null ? e.DuAnBuoc.Buoc.Ten : null))
                    : duAnBuocQuery
                        .Where(dab => dab.Id == e.BuocId)
                        .Select(dab => dab.TenBuoc ?? (dab.Buoc != null ? dab.Buoc.Ten : null))
                        .FirstOrDefault(),
                TenGoiThau = e.Ten,
                GiaGoiThau = e.GiaTri,
                TenNguonVon = e.NguonVon != null ? e.NguonVon.Ten : null,
                TenHinhThucLuaChonNhaThau = e.HinhThucLuaChonNhaThau != null ? e.HinhThucLuaChonNhaThau.Ten : null,
                TenPhuongThucLuaChonNhaThau = e.PhuongThucLuaChonNhaThau != null ? e.PhuongThucLuaChonNhaThau.Ten : null,
                ThoiGianToChucLuaChonNhaThau = e.ThoiGianLuaNhaThau,
                TenLoaiHopDong = e.LoaiHopDong != null ? e.LoaiHopDong.Ten : null,
            })
            .ToListAsync(cancellationToken);

        return rows.Select((row, index) => new TinhHinhThucHienDauThauExportDto
        {
            Stt = index + 1,
            TenDuAn = row.TenDuAn,
            TenBuoc = row.TenBuoc,
            TenGoiThau = row.TenGoiThau,
            GiaGoiThau = row.GiaGoiThau,
            TenNguonVon = row.TenNguonVon,
            TenHinhThucLuaChonNhaThau = row.TenHinhThucLuaChonNhaThau,
            TenPhuongThucLuaChonNhaThau = row.TenPhuongThucLuaChonNhaThau,
            ThoiGianToChucLuaChonNhaThau = row.ThoiGianToChucLuaChonNhaThau,
            TenLoaiHopDong = row.TenLoaiHopDong,
        }).ToList();
    }
}
