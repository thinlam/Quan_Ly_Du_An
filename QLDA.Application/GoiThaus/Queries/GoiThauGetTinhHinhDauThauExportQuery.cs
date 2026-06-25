using Microsoft.EntityFrameworkCore;
using QLDA.Application.GoiThaus.DTOs;
using QLDA.Domain.Enums;

namespace QLDA.Application.GoiThaus.Queries;

public record GoiThauGetTinhHinhDauThauExportQuery : IRequest<List<TinhHinhThucHienDauThauExportDto>>
{
    public required TinhHinhThucHienDauThauLoai Loai { get; set; }
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
        var duAnBuocQuery = _duAnBuoc.GetQueryableSet().AsNoTracking();

        var queryable = _goiThau.GetOrderedSet()
            .Include(e => e.KetQuaTrungThau)
            .Include(e => e.HopDong)
            .AsQueryable();

        queryable = request.Loai switch
        {
            TinhHinhThucHienDauThauLoai.ChuaCoKetQua => queryable.Where(e => e.KetQuaTrungThau == null && e.HopDong == null),
            TinhHinhThucHienDauThauLoai.CoKetQua => queryable.Where(e => e.KetQuaTrungThau != null && e.HopDong == null),
            TinhHinhThucHienDauThauLoai.DaLenHopDong => queryable.Where(e => e.KetQuaTrungThau != null && e.HopDong != null),
            _ => throw new ManagedException("Loại tab không hợp lệ cho export dữ liệu."),
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
