using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Mapping;
using QLDA.Application.HoSoMoiThauDienTus.DTOs;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Constants;
using QLDA.Domain.Enums;

namespace QLDA.Application.HoSoMoiThauDienTus.Queries;

public record HoSoMoiThauDienTuGetDanhSachQuery(HoSoMoiThauDienTuSearchDto SearchDto) 
    : AggregateRootPagination, IMayHaveGlobalFilter, IRequest<PaginatedList<HoSoMoiThauDienTuDto>> {
    public string? GlobalFilter { get; set; }
}

internal class HoSoMoiThauDienTuGetDanhSachQueryHandler : IRequestHandler<HoSoMoiThauDienTuGetDanhSachQuery, PaginatedList<HoSoMoiThauDienTuDto>> {
    private readonly IRepository<HoSoMoiThauDienTu, Guid> HoSoMoiThauDienTu;
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem ;

    public HoSoMoiThauDienTuGetDanhSachQueryHandler(IServiceProvider serviceProvider) {
        HoSoMoiThauDienTu = serviceProvider.GetRequiredService<IRepository<HoSoMoiThauDienTu, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
    }

    public async Task<PaginatedList<HoSoMoiThauDienTuDto>> Handle(HoSoMoiThauDienTuGetDanhSachQuery request,
        CancellationToken cancellationToken = default) {
        var queryable = HoSoMoiThauDienTu.GetQueryableSet()
            .AsNoTracking()
            .Include(e => e.DuAn)
            .Include(e => e.Buoc)
            .Include(e => e.HinhThucLuaChonNhaThau)
            .Include(e => e.GoiThau)
            .Include(e => e.TrangThaiPheDuyet)
            .Include(e => e.ToTrinh)
            .Include(e => e.QuyetDinh)
            .WhereGlobalFilter(
                request,  // Truyền request (implement IMayHaveGlobalFilter)
                e => e.ThoiGianThucHien
            );

        // Filter by DuAnId if provided
        if (request.SearchDto.DuAnId.HasValue) {
            queryable = queryable.Where(e => e.DuAnId == request.SearchDto.DuAnId);
        }
        if (request.SearchDto.LoaiDuAnTheoNamId > 0) {
            queryable = queryable.Where(e => e.DuAn!.LoaiDuAnTheoNamId == request.SearchDto.LoaiDuAnTheoNamId);
        }

        // Filter by GoiThauId if provided
        if (request.SearchDto.GoiThauId.HasValue) {
            queryable = queryable.Where(e => e.GoiThauId == request.SearchDto.GoiThauId);
        }

        return await queryable
             .Select(e => new HoSoMoiThauDienTuDto()
             {
                 Id = e.Id,
                 DuAnId = e.DuAnId,
                 BuocId = e.BuocId,
                 TenDuAn = e.DuAn.TenDuAn,
                 TenBuoc = e.Buoc.TenBuoc,
                 HinhThucLuaChonNhaThauId = e.HinhThucLuaChonNhaThauId,
                 ThamDinh = e.ThamDinh??false,
                 TenHinhThucLuaChonNhaThau = e.HinhThucLuaChonNhaThau.Ten,
                 GoiThauId = e.GoiThauId,
                 TenGoiThau = e.GoiThau.Ten,
                 GiaTri = e.GiaTri,
                 ThoiGianThucHien = e.ThoiGianThucHien,
                 TrangThaiDangTai = e.TrangThaiDangTai,
                 TrangThaiId = e.TrangThaiId,
                 TenTrangThai = e.TrangThaiId == null ? TrangThaiPheDuyetCodes.Default.TenDuThao : e.TrangThaiPheDuyet.Ten,
                
                 DanhSachTepDinhKem = TepDinhKem.GetQueryableSet()
                    .Where(i => i.GroupId == e.Id.ToString()
                    || (e.ToTrinh  != null && i.GroupId == e.QuyetDinh.Id.ToString() && i.GroupType == EGroupType.HoSoMoiThauDienTuToTrinh.ToString() )
                    || (e.QuyetDinh != null && i.GroupId == e.ToTrinh.Id.ToString() && i.GroupType == EGroupType.HoSoMoiThauDienTuQuyetDinh.ToString() ))

                    .Select(i => i.ToDto()).ToList()
             })
            //.Select(e => e.ToDto(e.))
            .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken);
    }
}
