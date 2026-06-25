using Microsoft.EntityFrameworkCore;
using QLDA.Application.GoiThaus.DTOs;

namespace QLDA.Application.GoiThaus.Queries;

public record GoiThauGetTinhHinhDauThauExportQuery : IRequest<List<TinhHinhThucHienDauThauExportDto>>
{
    /// <summary>
    /// Tab filter: 1 / 2 / 3. Null hoặc 0 = PrintController xuất 3 sheet (mỗi tab một sheet).
    /// </summary>
    public int? Loai { get; set; }
}

internal class GoiThauGetTinhHinhDauThauExportQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<GoiThauGetTinhHinhDauThauExportQuery, List<TinhHinhThucHienDauThauExportDto>>
{
    private readonly IRepository<GoiThau, Guid> _goiThau =
        serviceProvider.GetRequiredService<IRepository<GoiThau, Guid>>();
    private readonly IRepository<DuAnBuoc, int> _duAnBuoc =
        serviceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();

    public async Task<List<TinhHinhThucHienDauThauExportDto>> Handle(
        GoiThauGetTinhHinhDauThauExportQuery request,
        CancellationToken cancellationToken = default)
    {
        ManagedException.ThrowIf(request.Loai is int loai && loai is not (0 or 1 or 2 or 3),
            "Loại tab không hợp lệ. Chỉ chấp nhận giá trị 1 (Chưa có kết quả), 2 (Có kết quả), 3 (Đã lên hợp đồng), hoặc bỏ trống để xuất cả 3 tab.");

        var duAnBuocQuery = _duAnBuoc.GetQueryableSet().AsNoTracking();

        var queryable = _goiThau.GetOrderedSet()
            .Include(e => e.KetQuaTrungThau)
            .Include(e => e.HopDong)
            .AsQueryable();

        queryable = request.Loai switch
        {
            1 => queryable.Where(e => e.KetQuaTrungThau == null && e.HopDong == null),
            2 => queryable.Where(e => e.KetQuaTrungThau != null && e.HopDong == null),
            3 => queryable.Where(e => e.KetQuaTrungThau != null && e.HopDong != null),
            _ => queryable
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
