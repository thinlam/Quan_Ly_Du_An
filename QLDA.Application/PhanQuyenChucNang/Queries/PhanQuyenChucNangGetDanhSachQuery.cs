using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Mapping;
using QLDA.Application.PhanQuyenChucNangs.DTOs;
using QLDA.Domain.Constants;

namespace QLDA.Application.PhanQuyenChucNangs.Queries;

public record PhanQuyenChucNangDanhSachQuery : AggregateRootPagination, IMayHaveGlobalFilter, IRequest<PaginatedList<PhanQuyenChucNangDto>>
{
 
    public string? GlobalFilter { get; set; }
    public bool IsNoTracking { get; set; }
    public string? MaChucNang { get; set; } = string.Empty; // DuAn.TaoMoi/ DuAn.Sua/GoiThau
    public bool SuDung { get; set; }
    public string? ChucNang { get; set; }
    public int? Level { get; set; }   // PhanQuyenChucNangLevel NguoiDungMacDinhID, NguoiDungChiDinh, TheoChucVu
    public long? LevelId { get; set; }   //PhongBanId, NguoiDungId, ChucVuId, 
}

internal class
    PhanQuyenChucNangDanhSachQueryHandler(IServiceProvider serviceProvider) : IRequestHandler<PhanQuyenChucNangDanhSachQuery,
    PaginatedList<PhanQuyenChucNangDto>>
{
    private readonly IRepository<PhanQuyenChucNang, int> _PhanQuyenChucNang = serviceProvider.GetRequiredService<IRepository<PhanQuyenChucNang, int>>();
    private readonly IRepository<NguoiDungMacDinhTheoPhong, Guid> _PhongBanNguoiDungMacDinh = serviceProvider.GetRequiredService<IRepository<NguoiDungMacDinhTheoPhong, Guid>>();
    private readonly IRepository<UserMaster, long> _users = serviceProvider.GetRequiredService<IRepository<UserMaster, long>>();

    public async Task<PaginatedList<PhanQuyenChucNangDto>> Handle(PhanQuyenChucNangDanhSachQuery request,
        CancellationToken cancellationToken = default)
    {
        var queryable = _PhanQuyenChucNang.GetQueryableSet().Include(x=>x.DanhSachChiTiet)
            .WhereIf(request.SuDung, e => e.SuDung == request.SuDung)
            .WhereIf(!string.IsNullOrEmpty(request.MaChucNang), e => e.MaChucNang == request.MaChucNang)
            .WhereIf(!string.IsNullOrEmpty(request.ChucNang), e => e.ChucNang == request.ChucNang)
            .WhereIf(request.Level != null, e => (int)e.Level! == request.Level!.Value)

            ;
        try
        {
            var query = queryable
           .Select(e => new PhanQuyenChucNangDto
           {
               Id = e.Id,
               MaChucNang = e.MaChucNang ?? string.Empty,
               Level = e.Level,
               ChucNang = e.ChucNang,
               SuDung = e.SuDung,
               DanhSachChiTiet = e.DanhSachChiTiet!.Select(x => new PhanQuyenChucNangCapDoDto() {
                    LevelId = x.LevelId,
                    NguoiDungMacDinh = x.NguoiDungMacDinh,
                    TenNguoiDungMacDinh =  e.Level == PhanQuyenChucNangLevel.PhongBan &&
                    x.NguoiDungMacDinh == true
                    ? ( from md in _PhongBanNguoiDungMacDinh.GetQueryableSet()
                        join u in _users.GetQueryableSet()  on md.NguoiDungId equals u.UserPortalId
                        where md.PhongBanId == x.LevelId
                        select u.HoTen
                    ).FirstOrDefault() : null }).ToList()
            });

            var pagedResult = await query.PaginatedListAsync(
                request.Skip(),
                request.Take(),
                cancellationToken);

            foreach (var item in pagedResult.Data)
            {
                item.TenLevel = item.Level?.GetDescription();
            }
        return pagedResult;
        }
        catch (Exception)
        {

            throw;
        }


    }
}