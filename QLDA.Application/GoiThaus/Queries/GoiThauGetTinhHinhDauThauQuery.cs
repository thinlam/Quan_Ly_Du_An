using Microsoft.EntityFrameworkCore;
using QLDA.Application.GoiThaus;
using QLDA.Application.GoiThaus.DTOs;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.Providers;

public record GoiThauGetTinhHinhDauThauQuery(TinhHinhDauThauSearchDto SearchDto) : AggregateRootPagination, IRequest<PaginatedList<GoiThauDto>> {
};
internal class GoiThauGetDanhSachQueryHandler : IRequestHandler<GoiThauGetTinhHinhDauThauQuery, PaginatedList<GoiThauDto>>
{
    private readonly IRepository<GoiThau, Guid> GoiThau;
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem;
    private readonly IRepository<HopDong, Guid> HopDong;
    private readonly IRepository<KetQuaTrungThau, Guid> KetQuaTrungThau;
    private readonly IRepository<DuAn, Guid> _duAn;
    private readonly IUserProvider _userProvider;
    private readonly IPolicyProvider _policyProvider;

    public GoiThauGetDanhSachQueryHandler(IServiceProvider serviceProvider)
    {
        GoiThau = serviceProvider.GetRequiredService<IRepository<GoiThau, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
        HopDong = serviceProvider.GetRequiredService<IRepository<HopDong, Guid>>();
        KetQuaTrungThau = serviceProvider.GetRequiredService<IRepository<KetQuaTrungThau, Guid>>();
        _duAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _policyProvider = serviceProvider.GetRequiredService<IPolicyProvider>();
    }

    public async Task<PaginatedList<GoiThauDto>> Handle(    GoiThauGetTinhHinhDauThauQuery request,    CancellationToken cancellationToken = default)
    {
        var queryable = GoiThau.GetOrderedSet()
            .Include(e => e.KetQuaTrungThau)
            .Include(e => e.HopDong)
            .AsQueryable()
            .ApplyTinhHinhDauThauFilters(
                request.SearchDto.DuAnId,
                request.SearchDto.GiaiDoanId)
            .ApplyTinhHinhDauThauNamFilter(request.SearchDto.Nam)
            .ApplyTinhHinhDauThauTabFilter(request.SearchDto.TrangThai);
        queryable = queryable.AsNoTracking();

        //  var entities = await queryable.ToListAsync(cancellationToken);
        var entities = await queryable
                    .AsNoTracking()
                    .Select(e => new GoiThauDto
                    {
                        Id = e.Id,
                        Ten = e.Ten,
                        DuAnId = e.DuAnId,
                        BuocId = e.BuocId,
                        GiaTri = e.GiaTri,
                        DaDuyet = e.DaDuyet,
                        NguonVonId = e.NguonVonId,
                        LoaiHopDongId = e.LoaiHopDongId,
                        TuyChonMuaThem = e.TuyChonMuaThem,
                        ThoiGianHopDong = e.ThoiGianHopDong,
                        ThoiGianLuaNhaThau = e.ThoiGianLuaNhaThau,
                        GiamSatHoatDongDauThau = e.GiamSatHoatDongDauThau,
                        KeHoachLuaChonNhaThauId = e.KeHoachLuaChonNhaThauId, //SoKeHoachLuaChonNhaThau
                        ThoiGianThucHienGoiThau = e.ThoiGianThucHienGoiThau,
                        HinhThucLuaChonNhaThauId = e.HinhThucLuaChonNhaThauId,//TenHinhThucLuaChonNhaThau
                        PhuongThucLuaChonNhaThauId = e.PhuongThucLuaChonNhaThauId, //TenPhuongThucGoiNhaThau
                        TomTatCongViecChinhGoiThau = e.TomTatCongViecChinhGoiThau,
                        ThoiGianBatDauToChucLuaChonNhaThau = e.ThoiGianBatDauToChucLuaChonNhaThau,
                        DanhSachTepDinhKem = TepDinhKem.GetQueryableSet().Where(i => i.GroupId == e.Id.ToString()).Select(i => i.ToDto()).ToList(),
                    })
                    .ToListAsync(cancellationToken);
        return PaginatedList<GoiThauDto>.Create(entities,  request.SearchDto.Skip(), request.SearchDto.Take());  
    }

}
