using System.Linq.Dynamic.Core;
using QLDA.Application.Common.Enums;
using QLDA.Application.Common.Mapping;

namespace QLDA.Application.Common.Queries;

public record DanhMucGetDanhSachQuery : AggregateRootPagination, IRequest<object> {
    public bool GetAll { get; set; }
    public bool Order { get; set; }

    /// <summary>
    /// Dùng khi không thể lọc theo thuộc tính chung
    /// </summary>
    public string? GlobalFilter { get; set; }

    public List<long>? Ids { get; set; }
    public EDanhMuc DanhMuc { get; set; }
}

internal class DanhMucGetDanhSachQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<DanhMucGetDanhSachQuery, object> {
    private readonly IRepository<DanhMucBuoc, int> DanhMucBuoc =
        serviceProvider.GetRequiredService<IRepository<DanhMucBuoc, int>>();

    private readonly IRepository<DanhMucPhuongThucKySo, int> DanhMucPhuongThucKySo =
        serviceProvider.GetRequiredService<IRepository<DanhMucPhuongThucKySo, int>>();

    private readonly IRepository<DanhMucBuocTrangThaiTienDo, int> DanhMucBuocTrangThaiTienDo =
        serviceProvider.GetRequiredService<IRepository<DanhMucBuocTrangThaiTienDo, int>>();

    private readonly IRepository<DanhMucChuDauTu, int> DanhMucChuDauTu =
        serviceProvider.GetRequiredService<IRepository<DanhMucChuDauTu, int>>();

    private readonly IRepository<DanhMucHinhThucDauTu, int> DanhMucHinhThucDauTu =
        serviceProvider.GetRequiredService<IRepository<DanhMucHinhThucDauTu, int>>();

    private readonly IRepository<DanhMucHinhThucQuanLy, int> DanhMucHinhThucQuanLy =
        serviceProvider.GetRequiredService<IRepository<DanhMucHinhThucQuanLy, int>>();

    private readonly IRepository<DanhMucLinhVuc, int> DanhMucLinhVuc =
        serviceProvider.GetRequiredService<IRepository<DanhMucLinhVuc, int>>();

    private readonly IRepository<DanhMucLoaiDuAn, int> DanhMucLoaiDuAn =
        serviceProvider.GetRequiredService<IRepository<DanhMucLoaiDuAn, int>>();

    private readonly IRepository<DanhMucLoaiDuAnTheoNam, int> DanhMucLoaiDuAnTheoNam =
        serviceProvider.GetRequiredService<IRepository<DanhMucLoaiDuAnTheoNam, int>>();

    private readonly IRepository<DanhMucNguonVon, int> DanhMucNguonVon =
        serviceProvider.GetRequiredService<IRepository<DanhMucNguonVon, int>>();

    private readonly IRepository<DanhMucNhomDuAn, int> DanhMucNhomDuAn =
        serviceProvider.GetRequiredService<IRepository<DanhMucNhomDuAn, int>>();

    private readonly IRepository<DanhMucTrangThaiDuAn, int> DanhMucTrangThaiDuAn =
        serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiDuAn, int>>();

    private readonly IRepository<DanhMucTrangThaiTienDo, int> DanhMucTrangThaiTienDo =
        serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiTienDo, int>>();

    private readonly IRepository<DanhMucChucVu, int> DanhMucChucVu =
        serviceProvider.GetRequiredService<IRepository<DanhMucChucVu, int>>();

    private readonly IRepository<DanhMucLoaiVanBan, int> DanhMucLoaiVanBan =
        serviceProvider.GetRequiredService<IRepository<DanhMucLoaiVanBan, int>>();

    private readonly IRepository<DanhMucHinhThucLuaChonNhaThau, int> DanhMucHinhThucLuaChonNhaThau =
        serviceProvider.GetRequiredService<IRepository<DanhMucHinhThucLuaChonNhaThau, int>>();

    private readonly IRepository<DanhMucLoaiHopDong, int> DanhMucLoaiHopDong =
        serviceProvider.GetRequiredService<IRepository<DanhMucLoaiHopDong, int>>();

    private readonly IRepository<DanhMucLoaiGoiThau, int> DanhMucLoaiGoiThau =
        serviceProvider.GetRequiredService<IRepository<DanhMucLoaiGoiThau, int>>();

    private readonly IRepository<DanhMucPhuongThucLuaChonNhaThau, int> DanhMucPhuongThucLuaChonNhaThau =
        serviceProvider.GetRequiredService<IRepository<DanhMucPhuongThucLuaChonNhaThau, int>>();

    private readonly IRepository<DanhMucTinhTrangKhoKhan, int> DanhMucTinhTrangKhoKhan =
        serviceProvider.GetRequiredService<IRepository<DanhMucTinhTrangKhoKhan, int>>();

    private readonly IRepository<DanhMucGiaiDoan, int> DanhMucGiaiDoan =
        serviceProvider.GetRequiredService<IRepository<DanhMucGiaiDoan, int>>();

    private readonly IRepository<DanhMucMucDoKhoKhan, int> DanhMucMucDoKhoKhan =
        serviceProvider.GetRequiredService<IRepository<DanhMucMucDoKhoKhan, int>>();

    private readonly IRepository<DanhMucTinhTrangThucHienLcnt, int> DanhMucTinhTrangThucHienLcnt =
        serviceProvider.GetRequiredService<IRepository<DanhMucTinhTrangThucHienLcnt, int>>();

    private readonly IRepository<DmCapDoCntt, int> DmCapDoCntt =
        serviceProvider.GetRequiredService<IRepository<DmCapDoCntt, int>>();

    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> DanhMucTrangThaiPheDuyet =
        serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();

    public async Task<object> Handle(DanhMucGetDanhSachQuery request,
        CancellationToken cancellationToken) {
        return request.DanhMuc switch {
            EDanhMuc.DanhMucBuocTrangThaiTienDo => await
                GetDanhMucAsync<DanhMucBuocTrangThaiTienDo, int, DanhMucDto<int>>(DanhMucBuocTrangThaiTienDo, request,
                    cancellationToken),
            EDanhMuc.DanhMucChuDauTu => await GetDanhMucAsync<DanhMucChuDauTu, int, DanhMucDto<int>>(DanhMucChuDauTu,
                request, cancellationToken),
            EDanhMuc.DanhMucHinhThucDauTu => await GetDanhMucAsync<DanhMucHinhThucDauTu, int, DanhMucDto<int>>(
                DanhMucHinhThucDauTu, request, cancellationToken),
            EDanhMuc.DanhMucHinhThucQuanLy => await GetDanhMucAsync<DanhMucHinhThucQuanLy, int, DanhMucDto<int>>(
                DanhMucHinhThucQuanLy, request, cancellationToken),
            EDanhMuc.DanhMucLinhVuc => await GetDanhMucAsync<DanhMucLinhVuc, int, DanhMucDto<int>>(DanhMucLinhVuc,
                request, cancellationToken),
            EDanhMuc.DanhMucLoaiDuAn => await GetDanhMucAsync<DanhMucLoaiDuAn, int, DanhMucDto<int>>(DanhMucLoaiDuAn,
                request, cancellationToken),
            EDanhMuc.DanhMucLoaiDuAnTheoNam => await GetDanhMucAsync<DanhMucLoaiDuAnTheoNam, int, DanhMucDto<int>>(
                DanhMucLoaiDuAnTheoNam, request, cancellationToken),
            EDanhMuc.DanhMucNhomDuAn => await GetDanhMucAsync<DanhMucNhomDuAn, int, DanhMucDto<int>>(DanhMucNhomDuAn,
                request, cancellationToken),
            EDanhMuc.DanhMucTrangThaiTienDo => await GetDanhMucAsync<DanhMucTrangThaiTienDo, int, DanhMucDto<int>>(
                DanhMucTrangThaiTienDo, request, cancellationToken),
            EDanhMuc.DanhMucTrangThaiDuAn => await GetDanhMucAsync<DanhMucTrangThaiDuAn, int, DanhMucDto<int>>(
                DanhMucTrangThaiDuAn, request, cancellationToken),
            EDanhMuc.DanhMucChucVu => await GetDanhMucAsync<DanhMucChucVu, int, DanhMucDto<int>>(DanhMucChucVu, request,
                cancellationToken),
            EDanhMuc.DanhMucLoaiVanBan => await GetDanhMucAsync<DanhMucLoaiVanBan, int, DanhMucDto<int>>(
                DanhMucLoaiVanBan, request, cancellationToken),
            EDanhMuc.DanhMucHinhThucLuaChonNhaThau => await
                GetDanhMucAsync<DanhMucHinhThucLuaChonNhaThau, int, DanhMucDto<int>>(DanhMucHinhThucLuaChonNhaThau,
                    request, cancellationToken),
            EDanhMuc.DanhMucLoaiHopDong => await GetDanhMucAsync<DanhMucLoaiHopDong, int, DanhMucDto<int>>(
                DanhMucLoaiHopDong, request, cancellationToken),
            EDanhMuc.DanhMucLoaiGoiThau => await GetDanhMucAsync<DanhMucLoaiGoiThau, int, DanhMucDto<int>>(
                DanhMucLoaiGoiThau, request, cancellationToken),
            EDanhMuc.DanhMucPhuongThucLuaChonNhaThau => await
                GetDanhMucAsync<DanhMucPhuongThucLuaChonNhaThau, int, DanhMucDto<int>>(DanhMucPhuongThucLuaChonNhaThau,
                    request, cancellationToken),
            EDanhMuc.DanhMucTinhTrangKhoKhan => await GetDanhMucAsync<DanhMucTinhTrangKhoKhan, int, DanhMucDto<int>>(
                DanhMucTinhTrangKhoKhan, request, cancellationToken),
            EDanhMuc.DanhMucNguonVon => await GetDanhMucAsync<DanhMucNguonVon, int, DanhMucDto<int>>(DanhMucNguonVon,
                request, cancellationToken),
            EDanhMuc.DanhMucGiaiDoan => await GetDanhMucAsync<DanhMucGiaiDoan, int, DanhMucDto<int>>(DanhMucGiaiDoan,
                request, cancellationToken),
            EDanhMuc.DanhMucMucDoKhoKhan => await GetDanhMucAsync<DanhMucMucDoKhoKhan, int, DanhMucDto<int>>(
                DanhMucMucDoKhoKhan,
                request, cancellationToken),
            EDanhMuc.DanhMucTinhTrangThucHienLcnt => await GetDanhMucAsync<DanhMucTinhTrangThucHienLcnt, int, DanhMucDto<int>>(
                DanhMucTinhTrangThucHienLcnt,
                request, cancellationToken),
            EDanhMuc.DmCapDoCntt => await GetDanhMucAsync<DmCapDoCntt, int, DanhMucDto<int>>(
                DmCapDoCntt,
                request, cancellationToken),
            EDanhMuc.DanhMucPhuongThucKySo => await GetDanhMucAsync<DanhMucPhuongThucKySo, int, DanhMucDto<int>>(
            DanhMucPhuongThucKySo,
            request, cancellationToken),
            EDanhMuc.DanhMucTrangThaiPheDuyet => await GetDanhMucAsync<DanhMucTrangThaiPheDuyet, int, DanhMucDto<int>>(
                DanhMucTrangThaiPheDuyet,
                request, cancellationToken),
            _ => Enumerable.Empty<object>()
        };
    }

    private async Task<PaginatedList<TDto>> GetDanhMucAsync<TEntity, TKey, TDto>(
        IRepository<TEntity, TKey> repo,
        DanhMucGetDanhSachQuery request,
        CancellationToken cancellationToken = default)
        where TEntity : DanhMuc<TKey>, IAggregateRoot, IMayHaveStt, new()
        where TKey : notnull
        where TDto : DanhMucDto<TKey>, new() {
        var ids = request.Ids?.Select(e => e.ToString());
        var query = repo.GetQueryableSet()
            .Where(x => !x.IsDeleted)
            .WhereIf(request.Ids != null, e => ids!.Contains(e.Id!.ToString()) || e.Used || request.GetAll,
                e => request.GetAll || e.Used);


        string keyword = request.GlobalFilter?.ToLower() ?? string.Empty;
        string expression = string.Empty;

        if (request.GlobalFilter != null) {
            expression = "Ten.ToLower().Contains(@0) or MoTa.ToLower().Contains(@0)";
        }

        try {
            var predicate = DynamicExpressionParser.ParseLambda<TEntity, bool>(
                new ParsingConfig(), false, expression, keyword);
            query = query.Where(predicate);
        } catch {
            // ignored
        }

        if (request.Order)
            return await query
                .OrderBy(x => x.Stt)
                .Select(x => x.ToDanhMucDto<TEntity, TKey, TDto>())
                .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken);
        //Trường hợp enum thì không có stt
        return await query
            .Select(x => x.ToDanhMucDto<TEntity, TKey, TDto>())
            .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken);
    }
}