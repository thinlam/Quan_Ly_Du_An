using QLDA.Application.Common.Interfaces;
using QLDA.Application.Common.Mapping;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.TongHopVanBanQuyetDinhs.DTOs;
using QLDA.Domain.Enums;

namespace QLDA.Application.TongHopVanBanQuyetDinhs.Queries;

public record TongHopVanBanQuyetDinhGetListQuery : AggregateRootPagination,
    IRequest<PaginatedList<TongHopVanBanQuyetDinhDto>>,
    IFromDateToDate,
    IMayHaveGlobalFilter {
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? GlobalFilter { get; set; }
    public long? DonViId { get; set; }
    public EnumLoaiVanBanQuyetDinh? Loai { get; set; }
    public string? TrichYeu { get; set; }
    public DateOnly? TuNgay { get; set; }
    public DateOnly? DenNgay { get; set; }
    /// <summary>
    /// Loại dự án theo năm - tài chính
    /// </summary>
    /// <remarks>PMIS #9609</remarks>
    public int? LoaiDuAnTheoNamId { get; set; }
}

public record TongHopVanBanQuyetDinhGetListQueryHandler(IServiceProvider ServiceProvider) : IRequestHandler<TongHopVanBanQuyetDinhGetListQuery, PaginatedList<TongHopVanBanQuyetDinhDto>> {

    private readonly IRepository<VanBanQuyetDinh, Guid> VanBanQuyetDinh = ServiceProvider.GetRequiredService<IRepository<VanBanQuyetDinh, Guid>>();

    private readonly IRepository<TepDinhKem, Guid> TepDinhKem = ServiceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();

    public async Task<PaginatedList<TongHopVanBanQuyetDinhDto>> Handle(TongHopVanBanQuyetDinhGetListQuery request,
        CancellationToken cancellationToken) {

        #region Concat() => Union all (không loại bỏ trùng) / Union() => loại bỏ trùng

        var query = VanBanQuyetDinh.GetQueryableSet()
                .WhereIf(request.Loai.HasValue, e => e.Loai == request.Loai.ToString())
                .WhereIf(request.DuAnId.HasValue, e => e.DuAnId == request.DuAnId)
                .WhereIf(request.LoaiDuAnTheoNamId > 0, e => e.DuAn!.LoaiDuAnTheoNamId == request.LoaiDuAnTheoNamId)
                .WhereIf(request.BuocId > 0, e => e.BuocId == request.BuocId)
                .WhereIf(request.TrichYeu.IsNotNullOrWhitespace(), e => e.TrichYeu!.ToLower().Contains(request.TrichYeu!.ToLower()))
                .WhereIf(request.TuNgay.HasValue,
                    e => e.Ngay.HasValue && e.Ngay.Value >= request.TuNgay!.Value.ToStartOfDayUtc())
                .WhereIf(request.DenNgay.HasValue,
                    e => e.Ngay.HasValue && e.Ngay.Value <= request.DenNgay!.Value.ToEndOfDayUtc())
                .WhereGlobalFilter(
                    request,
                    e => e.So,
                    e => e.TrichYeu,
                    e => e.DuAn!.TenDuAn
                )
            ;

        #endregion

        return await query
            .Select(e => new TongHopVanBanQuyetDinhDto {
                Id = e.Id,
                DuAnId = e.DuAnId,
                BuocId = e.BuocId,
                So = e.So,
                Ngay = e.Ngay ?? e.NgayKy,
                TrichYeu = e.TrichYeu,
                TableName = e.Loai,
                Loai = e.Loai!.GetDescriptionFromName<EnumLoaiVanBanQuyetDinh>(),
                DanhSachTepDinhKem = TepDinhKem.GetOrderedSet()
                    .Where(f => f.GroupId == e.Id.ToString())
                    .Select(f => f.ToDto()).ToList()
            })
            .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken);

    }
}