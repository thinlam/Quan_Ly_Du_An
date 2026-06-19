using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common.Interfaces;
using QLDA.Application.Common.Mapping;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.KhoKhanVuongMacs.DTOs;
using QLDA.Domain.Enums;

namespace QLDA.Application.KhoKhanVuongMacs.Queries;

public record KhoKhanVuongMacGetDanhSachQuery : AggregateRootPagination, IMayHaveGlobalFilter, IRequest<PaginatedList<KhoKhanVuongMacDto>>, IFromDateToDate {
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? GlobalFilter { get; set; }
    public bool IsNoTracking { get; set; }

    public string? NoiDung { get; set; }
    /// <summary>
    /// Trạng thái xử lý
    /// </summary>
    public int? TinhTrangId { get; set; }
    /// <summary>
    /// Mức độ khó khăn
    /// </summary>
    public int? MucDoKhoKhanId { get; set; }
    /// <summary>
    /// Loại dự án
    /// </summary>
    public int? LoaiDuAnId { get; set; }
    /// <summary>
    /// Loại dự án theo năm - tài chính
    /// </summary>
    /// <remarks>PMIS #9609</remarks>
    public int? LoaiDuAnTheoNamId { get; set; }
    public DateOnly? TuNgay { get; set; }
    public DateOnly? DenNgay { get; set; }
    public long? LanhDaoPhuTrachId { get; set; }
    public long? DonViPhuTrachChinhId { get; set; }
    public long? DonViPhoiHopId { get; set; }
}

internal class
    KhoKhanVuongMacGetDanhSachQueryHandler : IRequestHandler<KhoKhanVuongMacGetDanhSachQuery,
    PaginatedList<KhoKhanVuongMacDto>> {
    private readonly IRepository<BaoCaoKhoKhanVuongMac, Guid> KhoKhanVuongMac;
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem;
    private readonly IAuthorizationManager _authManager;

    public KhoKhanVuongMacGetDanhSachQueryHandler(IServiceProvider serviceProvider) {
        KhoKhanVuongMac = serviceProvider.GetRequiredService<IRepository<BaoCaoKhoKhanVuongMac, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
        _authManager = serviceProvider.GetRequiredService<IAuthorizationManager>();
    }

    public async Task<PaginatedList<KhoKhanVuongMacDto>> Handle(KhoKhanVuongMacGetDanhSachQuery request,
        CancellationToken cancellationToken = default) {
        var queryable = _authManager.FilterVisible(KhoKhanVuongMac.GetQueryableSet(), AuthorizationResourceKeys.DuAn)
            .Where(e => !e.DuAn!.IsDeleted)
            .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
            .WhereIf(request.BuocId > 0, e => e.BuocId == request.BuocId)
            .WhereIf(request.TinhTrangId > 0, e => e.TinhTrangId == request.TinhTrangId)
            .WhereIf(request.MucDoKhoKhanId > 0, e => e.MucDoKhoKhanId == request.MucDoKhoKhanId)
            .WhereIf(request.LoaiDuAnId > 0, e => e.DuAn!.LoaiDuAnId == request.LoaiDuAnId)
            .WhereIf(request.LoaiDuAnTheoNamId > 0, e => e.DuAn!.LoaiDuAnTheoNamId == request.LoaiDuAnTheoNamId)
            .WhereIf(request.NoiDung.IsNotNullOrWhitespace(), e => e.NoiDung!.ToLower().Contains(request.NoiDung!.ToLower()))
            .WhereIf(request.TuNgay.HasValue,
                e => e.Ngay.HasValue && e.Ngay.Value >= request.TuNgay!.Value.ToStartOfDayUtc())
            .WhereIf(request.DenNgay.HasValue,
                e => e.Ngay.HasValue && e.Ngay.Value <= request.DenNgay!.Value.ToEndOfDayUtc())
            .WhereGlobalFilter(
                request,
                e => e.NoiDung,
                e => e.TinhTrang!.Ten
            );

        return await queryable
            .Select(e => new KhoKhanVuongMacDto() {
                Id = e.Id,
                DuAnId = e.DuAnId,
                BuocId = e.BuocId,
                NoiDung = e.NoiDung,
                Ngay = e.Ngay,
                TinhTrangId = e.TinhTrangId,
                MucDoKhoKhanId = e.MucDoKhoKhanId,
                HuongXuLy = e.HuongXuLy,
                DanhSachTepDinhKem = TepDinhKem.GetQueryableSet()
                    .Where(i => i.GroupId == e.Id.ToString() && i.GroupType == nameof(EGroupType.KhoKhanVuongMac))
                    .Select(i => i.ToDto()).ToList(),
                KetQua = new KetQuaXuLyDto() {
                    KetQuaXuLy = e.KetQuaXuLy,
                    NgayXuLy = e.NgayXuLy,
                    DanhSachTepDinhKem = TepDinhKem.GetQueryableSet()
                        .Where(i => i.GroupId == e.Id.ToString() && i.GroupType == nameof(EGroupType.KetQuaXuLyKhoKhanVuongMac))
                        .Select(i => i.ToDto()).ToList()
                },


                #region Thông tin dự án
                LoaiDuAnId = e.DuAn!.LoaiDuAnId,
                NgayBatDau = e.DuAn.NgayBatDau,
                LanhDaoPhuTrachId = e.DuAn.LanhDaoPhuTrachId,
                DonViPhuTrachChinhId = e.DuAn.DonViPhuTrachChinhId,
                DonViPhoiHopId = e.DuAn.DuAnChiuTrachNhiemXuLys!.Where(i => i.Loai == EChiuTrachNhiemXuLy.DonViPhoiHop).Select(i => i.RightId).FirstOrDefault(),

                #endregion
            })
            .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken: cancellationToken);
    }
}