using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QLDA.Application.Authorization;
using QLDA.Application.Common.Mapping;
using QLDA.Application.PhanQuyenChucNangs.DTOs;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities;

namespace QLDA.Application.PhanQuyenChucNangs.Queries;

public record PhanQuyenChucNangDanhSachQuery : AggregateRootPagination, IMayHaveGlobalFilter, IRequest<PaginatedList<PhanQuyenChucNangDto>>
{
 
    public string? GlobalFilter { get; set; }
    public bool IsNoTracking { get; set; }
    public string MaChucNang { get; set; } // DuAn.TaoMoi/ DuAn.Sua/GoiThau
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

    public async Task<PaginatedList<PhanQuyenChucNangDto>> Handle(PhanQuyenChucNangDanhSachQuery request,
        CancellationToken cancellationToken = default)
    {
        var queryable = _PhanQuyenChucNang.GetQueryableSet()
            .WhereIf(request.SuDung, e => e.SuDung == request.SuDung)
            .WhereIf(!string.IsNullOrEmpty(request.MaChucNang), e => e.MaChucNang == request.MaChucNang)
            .WhereIf(!string.IsNullOrEmpty(request.ChucNang), e => e.ChucNang == request.ChucNang)
            .WhereIf(request.Level >= 0, e => (int)e.Level == request.Level)

            ;
        try
        {
            var query = queryable
           .Select(e => new PhanQuyenChucNangDto
           {
               Id = e.Id,
               //  TenNhomQuyen = e.Quyen.NhomQuyen,
               //  TenQuyen = e.Quyen.Ten,
               Level = e.Level,
               LevelId = e.LevelId,
               MaChucNang = e.MaChucNang,
               ChucNang = e.ChucNang,
               SuDung = e.SuDung,
               NguoiDungMacDinh = e.NguoiDungMacDinh,
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
        catch (Exception ex)
        {

            throw;
        }
       

    }
}