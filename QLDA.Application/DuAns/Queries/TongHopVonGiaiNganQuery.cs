using Microsoft.EntityFrameworkCore;

using QLDA.Application.DuAns.DTOs;
using QLDA.Domain.Entities;

namespace QLDA.Application.DuAns.Queries;

public record TongHopVonGiaiNganQuery(int Nam, int LoaiDuAnId)
    : AggregateRootPagination, IRequest<List<BaoCaoDuAnDto>>
{
    public bool IsNoTracking { get; set; } = true;
}

internal class TongHopVonGiaiNganQueryHandler
    : IRequestHandler<TongHopVonGiaiNganQuery, List<BaoCaoDuAnDto>>
{

    private readonly IRepository<DuAn, Guid> _duAn;
    private readonly IRepository<NghiemThu, Guid> _nghiemThu;
    private readonly IRepository<ThanhToan, Guid> _thanhToan;
    private readonly IRepository<KeHoachVon, Guid> _keHoachVon;
    private readonly IRepository<UserMaster, long> _userMaster;
    private readonly IRepository<DmDonVi, long> _dmDonVi;
    private readonly IDapperRepository _dapper;

    public TongHopVonGiaiNganQueryHandler(IServiceProvider serviceProvider)
    {
        _duAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
        _dmDonVi = serviceProvider.GetRequiredService<IRepository<DmDonVi, long>>();
        _userMaster= serviceProvider.GetRequiredService<IRepository<UserMaster, long>>();
        _nghiemThu = serviceProvider.GetRequiredService<IRepository<NghiemThu, Guid>>();
        _thanhToan = serviceProvider.GetRequiredService<IRepository<ThanhToan, Guid>>();
        _keHoachVon = serviceProvider.GetRequiredService<IRepository<KeHoachVon, Guid>>();
        _dapper = serviceProvider.GetRequiredService<IDapperRepository>();
    }

    public async Task<List<BaoCaoDuAnDto>> Handle(
       TongHopVonGiaiNganQuery request,
       CancellationToken cancellationToken = default) {
        var query = _duAn.GetQueryableSet()
            .Include(d => d.GiaiDoanHienTai)
            .Include(d => d.BuocHienTai)
            .Include(d => d.DuAnChiuTrachNhiemXuLys)
            .AsNoTracking().Where(e => !e.IsDeleted);

        var result = await query
            .Where(d =>_keHoachVon.GetQueryableSet()
                    .Any(k =>!k.IsDeleted &&   k.Nam == request.Nam &&  k.DuAnId == d.Id)  ||
                _thanhToan.GetQueryableSet()
                    .Any(t => !t.IsDeleted &&  t.NgayHoaDon.HasValue
                            &&  t.NgayHoaDon.Value.Year == request.Nam &&    t.DuAnId == d.Id)
            )
            .Select(d => new BaoCaoDuAnDto {
                Id = d.Id,
                TenDuAn = d.TenDuAn,
                MaDuAn = d.MaDuAn,
                TenBuoc = d.BuocHienTai != null ? d.BuocHienTai.TenBuoc    : null,
                TenGiaiDoanHienTai = d.BuocHienTai != null && d.BuocHienTai.Buoc != null && d.BuocHienTai.Buoc.GiaiDoan != null
                    ? d.BuocHienTai.Buoc.GiaiDoan.Ten
                    : d.GiaiDoanHienTai != null ? d.GiaiDoanHienTai.Ten : null,
                // GiaiDoanHienTaiId (denormalized) có thể stale so với bước hiện tại
                // (xem docs/issues/fix-01) → lấy phase từ bước hiện tại làm nguồn chuẩn.
                GiaiDoanHienTaiId = d.BuocHienTai != null && d.BuocHienTai.Buoc != null
                    ? d.BuocHienTai.Buoc.GiaiDoanId
                    : d.GiaiDoanHienTaiId,
                PhongBanPhuTrach = _dmDonVi.GetQueryableSet()
                    .Where(x => x.Id == d.DonViPhuTrachChinhId)
                    .Select(x => x.TenDonVi).FirstOrDefault(),
                NguoiPhuTrach = _userMaster.GetQueryableSet()
                    .Where(x => x.UserPortalId == d.LanhDaoPhuTrachId)
                    .Select(x => x.HoTen).FirstOrDefault(),
                DonViPhoiHops = string.Empty,
                LoaiDuAnTheoNamId = d.LoaiDuAnTheoNamId,
                KeHoachVon = _keHoachVon.GetQueryableSet()
                    .Where(k =>   !k.IsDeleted &&  k.Nam == request.Nam && k.DuAnId == d.Id)
                    .Sum(k =>(k.SoVonDieuChinh ?? 0) != 0   ? (k.SoVonDieuChinh ?? 0) : k.SoVon),

                GiaTriGiaiNgan = _thanhToan.GetQueryableSet()
                    .Where(t =>
                        !t.IsDeleted &&
                        t.DuAnId == d.Id &&
                        t.NgayHoaDon.HasValue &&
                        t.NgayHoaDon.Value.Year == request.Nam)
                    .Sum(t => (long?)t.GiaTri) ?? 0
            }).ToListAsync(cancellationToken);

        var duAnIds = result.Select(x => x.Id).ToList();

        if (duAnIds.Count > 0) {
            // Lấy các quan hệ dự án - đơn vị
            var duAnDonVis = await _duAn.GetQueryableSet()
                .Where(d => duAnIds.Contains(d.Id))
                .SelectMany(d => d.DuAnChiuTrachNhiemXuLys! .Select(x => new { DuAnId = d.Id,  x.RightId    }))
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // Lấy tên đơn vị
            var donViIds = duAnDonVis
                .Select(x => x.RightId).Distinct().ToList();

            var donVis = await _dmDonVi.GetQueryableSet()
                 .Select(x => new {  x.Id, x.TenDonVi
                }).AsNoTracking().ToListAsync(cancellationToken);

            var donViPhoiHopByDuAn = duAnDonVis
            .Join( donVis,   x => x.RightId,  dv => dv.Id, (x, dv) => new { x.DuAnId,   dv.TenDonVi   })
           .GroupBy(x => x.DuAnId)
           .ToDictionary( g => g.Key, g => string.Join(   ", ",
                                        g.Select(x => x.TenDonVi).Where(x => !string.IsNullOrWhiteSpace(x)) .Distinct()) );

            foreach (var item in result) {
                item.DonViPhoiHops = donViPhoiHopByDuAn.TryGetValue(
                        item.Id, out var tenDonVi) ? tenDonVi  : string.Empty;
            }
        }

        return result;
    }

}
