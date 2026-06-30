using QLDA.Application.Authorization;
using QLDA.Application.Common.Constants;
using QLDA.Application.Common.Interfaces;
using QLDA.Application.Common.Mapping;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.TongHopVanBanQuyetDinhs.DTOs;
using QLDA.Domain.Enums;
using QLDA.Domain.Interfaces;

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
    public string? CoQuanQuyetDinh { get; set; }
}

public record TongHopVanBanQuyetDinhGetListQueryHandler(IServiceProvider ServiceProvider) : IRequestHandler<TongHopVanBanQuyetDinhGetListQuery, PaginatedList<TongHopVanBanQuyetDinhDto>> {

    private readonly IRepository<VanBanQuyetDinh, Guid> VanBanQuyetDinh = ServiceProvider.GetRequiredService<IRepository<VanBanQuyetDinh, Guid>>();
    private readonly IRepository<VanBanPhapLy, Guid> VanBanPhapLy = ServiceProvider.GetRequiredService<IRepository<VanBanPhapLy, Guid>>();

    private readonly IRepository<TepDinhKem, Guid> TepDinhKem = ServiceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
    private readonly IAuthorizationManager _authManager = ServiceProvider.GetRequiredService<IAuthorizationManager>();

    public async Task<PaginatedList<TongHopVanBanQuyetDinhDto>> Handle(TongHopVanBanQuyetDinhGetListQuery request,
        CancellationToken cancellationToken) {

        #region Concat() => Union all (không loại bỏ trùng) / Union() => loại bỏ trùng

        var query = _authManager.FilterVisible(VanBanQuyetDinh.GetQueryableSet(), AuthorizationResourceKeys.DuAn)
                .WhereIf(request.Loai.HasValue, e => e.Loai == request.Loai.ToString())
                .WhereIf(request.DuAnId.HasValue, e => e.DuAnId == request.DuAnId)
                .WhereIf(request.LoaiDuAnTheoNamId > 0, e => e.DuAn!.LoaiDuAnTheoNamId == request.LoaiDuAnTheoNamId)
                .WhereIf(request.BuocId > 0, e => e.BuocId == request.BuocId)
                .WhereIf(!string.IsNullOrEmpty(request.CoQuanQuyetDinh), e => e.CoQuanQuyetDinh.Contains(request.CoQuanQuyetDinh))  
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
        // 2. Query cho VanBanPhapLy (Có thêm điều kiện ChuDauuId)
        /* var qPhapLy = VanBanPhapLy.GetQueryableSet()
                 .WhereIf(request.Loai.HasValue, e => e.Loai == request.Loai.ToString())
                 .WhereIf(request.DuAnId.HasValue, e => e.DuAnId == request.DuAnId)
                 .WhereIf(request.LoaiDuAnTheoNamId > 0, e => e.DuAn!.LoaiDuAnTheoNamId == request.LoaiDuAnTheoNamId)
                 .WhereIf(request.BuocId > 0, e => e.BuocId == request.BuocId)
                 // Lọc theo Chủ đầu tư chỉ có ở Văn bản pháp lý
                 .WhereIf(request.ChuDauuId > 0, e => e.ChuDauTuId == request.ChuDauuId)
                 .WhereIf(request.TrichYeu.IsNotNullOrWhitespace(), e => e.TrichYeu!.ToLower().Contains(request.TrichYeu!.ToLower()))
                 .WhereIf(request.TuNgay.HasValue, e => e.Ngay.HasValue && e.Ngay.Value >= request.TuNgay!.Value.ToStartOfDayUtc())
                 .WhereIf(request.DenNgay.HasValue, e => e.Ngay.HasValue && e.Ngay.Value <= request.DenNgay!.Value.ToEndOfDayUtc())
                 .WhereGlobalFilter(request, e => e.So, e => e.TrichYeu, e => e.DuAn!.TenDuAn)
                 .Select(e => new TongHopVanBanQuyetDinhDto
                 {
                     Id = e.Id,
                     DuAnId = e.DuAnId,
                     BuocId = e.BuocId,
                     So = e.So,
                     Ngay = e.Ngay ?? e.NgayKy,
                     TrichYeu = e.TrichYeu,
                     TableName = e.Loai,
                     Loai = e.Loai!.GetDescriptionFromName<EnumLoaiVanBanQuyetDinh>(),
                     DanhSachTepDinhKem = new List<TepDinhKemDto>()
                 });

         // 3. Concat 2 query lại với nhau (Tương đương UNION ALL trong SQL)
         var combinedQuery = qQuyetDinh.Concat(qPhapLy);

         //// 4. Phân trang trên tổng số dữ liệu đã gộp
         //var paginatedResult = await combinedQuery.PaginatedListAsync(request.Skip(), request.Take(), cancellationToken);

         //// 5. Điền thông tin Danh sách tệp đính kèm sau khi đã phân trang (Tối ưu hóa performance)
         //if (paginatedResult.Items.Any())
         //{
         //    var groupIds = paginatedResult.Items.Select(i => i.Id.ToString()).ToList();

         //    // Lấy tất cả tệp đính kèm thuộc danh sách Id hiện tại bằng 1 câu lệnh duy nhất
         //    var teps = await TepDinhKem.GetOrderedSet()
         //        .Where(f => groupIds.Contains(f.GroupId!))
         //        .ToListAsync(cancellationToken);

         //    // Map ngược lại vào DTO
         //    foreach (var item in paginatedResult.Items)
         //    {
         //        item.DanhSachTepDinhKem = teps
         //            .Where(f => f.GroupId == item.Id.ToString())
         //            .Select(f => f.ToDto())
         //            .ToList();
         //    }
         //}
        */
        #endregion

        var paginatedList = await query
    .Select(e => new TongHopVanBanQuyetDinhDto
    {
        Id = e.Id,
        DuAnId = e.DuAnId,
        BuocId = e.BuocId,
        So = e.So,
        Ngay = e.Ngay ?? e.NgayKy,
        TrichYeu = e.TrichYeu,
        TableName = e.Loai, // Đảm bảo trường này nhận giá trị từ DB
        Loai = e.Loai!.GetDescriptionFromName<EnumLoaiVanBanQuyetDinh>(),
        DanhSachTepDinhKem = TepDinhKem.GetOrderedSet()
            .Where(f => f.GroupId == e.Id.ToString())
            .Select(f => f.ToDto()).ToList()
    })
    .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken);
        // Bước 2: Duyệt qua danh sách đã có trong bộ nhớ để gán thuộc tính PartialView
        foreach (var item in paginatedList.Data) // Giả sử PaginatedList có thuộc tính .Items chứa danh sách dữ liệu
        {
            if (LoaiVanBanQuyetDinhConst.Dictionary.TryGetValue(item.TableName ?? string.Empty, out var value))
            {
                item.PartialView = value;
            }
        };
        return paginatedList;
    }
}