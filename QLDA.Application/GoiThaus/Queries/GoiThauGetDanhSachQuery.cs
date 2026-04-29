using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Extensions;
using QLDA.Application.Common.Mapping;
using QLDA.Application.Providers;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.GoiThaus.DTOs;
using System.Linq.Dynamic.Core;

namespace QLDA.Application.GoiThaus.Queries;

public record GoiThauGetDanhSachQuery(GoiThauSearchDto SearchDto) : AggregateRootPagination, IRequest<PaginatedList<GoiThauDto>> {
    public bool IsNoTracking { get; set; }
    public bool IsCbo { get; set; }
}

internal class
    GoiThauGetDanhSachQueryHandler : IRequestHandler<GoiThauGetDanhSachQuery,
    PaginatedList<GoiThauDto>> {
    private readonly IRepository<GoiThau, Guid> GoiThau;
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem;
    private readonly IRepository<HopDong, Guid> HopDong;
    private readonly IRepository<KetQuaTrungThau, Guid> KetQuaTrungThau;
    private readonly IRepository<DuAn, Guid> _duAn;
    private readonly IUserProvider _userProvider;
    private readonly IPolicyProvider _policyProvider;

    public GoiThauGetDanhSachQueryHandler(IServiceProvider serviceProvider) {
        GoiThau = serviceProvider.GetRequiredService<IRepository<GoiThau, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
        HopDong = serviceProvider.GetRequiredService<IRepository<HopDong, Guid>>();
        KetQuaTrungThau = serviceProvider.GetRequiredService<IRepository<KetQuaTrungThau, Guid>>();
        _duAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _policyProvider = serviceProvider.GetRequiredService<IPolicyProvider>();
    }

    public async Task<PaginatedList<GoiThauDto>> Handle(GoiThauGetDanhSachQuery request,
        CancellationToken cancellationToken = default) {
        var queryable = GoiThau.GetQueryableSet().AsNoTracking()
            .Where(e => e.DaDuyet)
            .Where(e => !e.DuAn!.IsDeleted)
            .ApplyDuAnChildVisibility(_duAn, _userProvider, _policyProvider, e => e.DuAnId)
            .WhereIf(request.SearchDto.DuAnId != null, e => e.DuAnId == request.SearchDto.DuAnId)
            .WhereIf(request.SearchDto.KeHoachLuaChonNhaThauId != null,
                e => e.KeHoachLuaChonNhaThauId == request.SearchDto.KeHoachLuaChonNhaThauId)
            .WhereIf(request.SearchDto.Ten.IsNotNullOrWhitespace(), e => e.Ten!.ToLower().Contains(request.SearchDto.Ten!.ToLower()))
            .WhereIf(request.SearchDto.BuocId > 0, e => e.BuocId == request.SearchDto.BuocId)
            .WhereIf(request.SearchDto.NguonVonId > 0, e => e.NguonVonId == request.SearchDto.NguonVonId)
            .WhereIf(request.SearchDto.LoaiHopDongId > 0, e => e.LoaiHopDongId == request.SearchDto.LoaiHopDongId)
            .WhereIf(request.SearchDto.PhuongThucLuaChonNhaThauId > 0,
                e => e.PhuongThucLuaChonNhaThauId == request.SearchDto.PhuongThucLuaChonNhaThauId)
            .WhereIf(request.SearchDto.HinhThucLuaChonNhaThauId > 0,
                e => e.HinhThucLuaChonNhaThauId == request.SearchDto.HinhThucLuaChonNhaThauId)
            .WhereGlobalFilter(
                request.SearchDto,
                e => e.Ten,
                e => e.NguonVon!.Ten,
                e => e.HinhThucLuaChonNhaThau!.Ten,
                e => e.LoaiHopDong!.Ten,
                e => e.PhuongThucLuaChonNhaThau!.Ten
            )
            .WhereFunc(request.IsCbo,
                q => q
                    .WhereIf(request.SearchDto.KetQuaTrungThauId.HasValue, e => e.KetQuaTrungThau!.Id == request.SearchDto.KetQuaTrungThauId || e.KetQuaTrungThau == null)
                    .WhereIf(request.SearchDto.HopDongId.HasValue, e => e.HopDong!.Id == request.SearchDto.HopDongId || e.HopDong == null),
                q => q
                    .WhereIf(request.SearchDto.HopDongId.HasValue, e => e.HopDong!.Id == request.SearchDto.HopDongId)

            )

            ;

        /*
         * Đoạn cmt bên dưới là các trường cần trả cho in excel
         */
        return await queryable
            .Select(e => new GoiThauDto() {
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
                #region Khu vực phải chú ý
                HopDongId = HopDong.GetQueryableSet().Where(h => h.GoiThauId == e.Id && !h.IsDeleted).Select(h => (Guid?)h.Id).FirstOrDefault() ,
                KetQuaTrungThauId = KetQuaTrungThau.GetQueryableSet().Where(k => k.GoiThauId == e.Id && !k.IsDeleted).Select(k => (Guid?)k.Id).FirstOrDefault(),
                #endregion
            })
            .PaginatedListAsync(request.SearchDto.Skip(), request.SearchDto.Take(), cancellationToken: cancellationToken);
    }
}