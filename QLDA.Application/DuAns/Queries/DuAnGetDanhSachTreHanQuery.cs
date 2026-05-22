using BuildingBlocks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Mapping;
using QLDA.Application.DuAns.DTOs;
using BuildingBlocks.CrossCutting.DateTimes;
using QLDA.WebApi.Models.DuAns;

namespace QLDA.Application.DuAns.Queries;

public record DuAnGetDanhSachTreHanQuery(DuAnSearchOverdueDto SearchDto) : AggregateRootPagination, IRequest<PaginatedList<DuAnTreHanDto>>;

public record DuAnGetDanhSachTreHanQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<DuAnGetDanhSachTreHanQuery, PaginatedList<DuAnTreHanDto>> {
    private readonly IRepository<DuAnBuoc, int> DuAnBuoc =
        serviceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();

    private readonly IRepository<DmDonVi, long> DanhMucDonVi =
        serviceProvider.GetRequiredService<IRepository<DmDonVi, long>>();

    private readonly IDateTimeProvider _dateTimeProvider = serviceProvider.GetRequiredService<IDateTimeProvider>();

    public async Task<PaginatedList<DuAnTreHanDto>> Handle(DuAnGetDanhSachTreHanQuery request,
        CancellationToken cancellationToken) {
        // var now = _dateTimeProvider.OffsetUtcNow;
        var query = DuAnBuoc.GetQueryableSet()
                .AsNoTracking()
                // đảm bảo bước chưa xóa và dự án chưa xóa
                .Where(e => !e.IsDeleted && e.DuAn != null && !e.DuAn.IsDeleted)
                // // chỉ những bước là bước hiện tại của dự án
                // .Where(e => e.DuAn!.BuocHienTaiId == e.Id)
                // có ngày dự kiến + ngày thực tế, và thực tế > dự kiến (trễ)
                .Where(e => e.NgayDuKienKetThuc.HasValue
                            && e.NgayThucTeKetThuc.HasValue
                            && e.NgayThucTeKetThuc.Value > e.NgayDuKienKetThuc.Value)
                //tìm đơn vị phụ trách chính
                .WhereIf(request.SearchDto.DonViPhuTrachChinhId > 0, e => e.DuAn!.DonViPhuTrachChinhId == request.SearchDto.DonViPhuTrachChinhId)
                //Tìm dự án thuộc bước có ngày dự kiến bắt đầu >= từ ngày 
                .WhereIf(request.SearchDto.DuKienTuNgay.HasValue, e => e.NgayDuKienBatDau >= request.SearchDto.DuKienTuNgay!.Value.ToStartOfDayUtc())
                //Tìm dự án thuộc bước có ngày dự kiến bắt đầu <= đến ngày 
                .WhereIf(request.SearchDto.DuKienDenNgay.HasValue, e => e.NgayDuKienBatDau!.Value <= request.SearchDto.DuKienDenNgay!.Value.ToEndOfDayUtc())
                //Tìm theo bước dự án
                .WhereIf(request.SearchDto.BuocId > 0, e => e.BuocId == request.SearchDto.BuocId)
                .WhereGlobalFilter(request.SearchDto,
                    e => e.DuAn!.TenDuAn,
                    e => e.Buoc!.Ten
                )
                .Select(e => new DuAnTreHanDto() {
                    DuAnId = e.DuAnId,
                    TenDuAn = e.DuAn!.TenDuAn,
                    DonViPhuTrachChinhId = e.DuAn.DonViPhuTrachChinhId,
                    NgayDuKienBatDau = e.NgayDuKienBatDau.ToDateOnlyVn(),
                    NgayDuKienKetThuc = e.NgayDuKienKetThuc.ToDateOnlyVn(),
                    NgayThucTeBatDau = e.NgayThucTeBatDau.ToDateOnlyVn(),
                    NgayThucTeKetThuc = e.NgayThucTeKetThuc.ToDateOnlyVn(),
                    TenBuoc = e.TenBuoc,
                    BuocId = e.BuocId,
                    TenDonViPhuTrachChinh = DanhMucDonVi.GetQueryableSet().Where(dv => dv.Id == e.DuAn!.DonViPhuTrachChinhId).Select(dv => dv.TenDonVi ?? "Không rõ").FirstOrDefault()
                })
            ;

        return await query.PaginatedListAsync(request.SearchDto.Skip(), request.SearchDto.Take(), cancellationToken);
    }
}