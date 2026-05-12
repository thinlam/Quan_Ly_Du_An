using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Enums;

namespace QLDA.Application.Common.Queries;

public class DanhMucGetQuery : IRequest<object> {
    public required string Id { get; set; }
    public bool Enum { get; set; }
    public EDanhMuc DanhMuc { get; set; }
    public bool ThrowIfNull { get; set; }
}

public class DanhMucGetQueryHandler(IServiceProvider serviceProvider) : IRequestHandler<DanhMucGetQuery, object> {
    private readonly IRepository<DanhMucBuoc, int> DanhMucBuoc = serviceProvider.GetRequiredService<IRepository<DanhMucBuoc, int>>();
    private readonly IRepository<DanhMucBuocTrangThaiTienDo, int> DanhMucBuocTrangThaiTienDo = serviceProvider.GetRequiredService<IRepository<DanhMucBuocTrangThaiTienDo, int>>();
    private readonly IRepository<DanhMucChuDauTu, int> DanhMucChuDauTu = serviceProvider.GetRequiredService<IRepository<DanhMucChuDauTu, int>>();
    private readonly IRepository<DanhMucHinhThucDauTu, int> DanhMucHinhThucDauTu = serviceProvider.GetRequiredService<IRepository<DanhMucHinhThucDauTu, int>>();
    private readonly IRepository<DanhMucHinhThucQuanLy, int> DanhMucHinhThucQuanLy = serviceProvider.GetRequiredService<IRepository<DanhMucHinhThucQuanLy, int>>();
    private readonly IRepository<DanhMucLinhVuc, int> DanhMucLinhVuc = serviceProvider.GetRequiredService<IRepository<DanhMucLinhVuc, int>>();
    private readonly IRepository<DanhMucLoaiDuAn, int> DanhMucLoaiDuAn = serviceProvider.GetRequiredService<IRepository<DanhMucLoaiDuAn, int>>();
    private readonly IRepository<DanhMucLoaiDuAnTheoNam, int> DanhMucLoaiDuAnTheoNam = serviceProvider.GetRequiredService<IRepository<DanhMucLoaiDuAnTheoNam, int>>();
    private readonly IRepository<DanhMucNguonVon, int> DanhMucNguonVon = serviceProvider.GetRequiredService<IRepository<DanhMucNguonVon, int>>();
    private readonly IRepository<DanhMucNhomDuAn, int> DanhMucNhomDuAn = serviceProvider.GetRequiredService<IRepository<DanhMucNhomDuAn, int>>();
    private readonly IRepository<DanhMucQuyTrinh, int> DanhMucQuyTrinh = serviceProvider.GetRequiredService<IRepository<DanhMucQuyTrinh, int>>();
    private readonly IRepository<DanhMucTrangThaiDuAn, int> DanhMucTrangThaiDuAn = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiDuAn, int>>();
    private readonly IRepository<DanhMucTrangThaiTienDo, int> DanhMucTrangThaiTienDo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiTienDo, int>>();
    private readonly IRepository<DanhMucManHinh, int> DanhMucManHinh = serviceProvider.GetRequiredService<IRepository<DanhMucManHinh, int>>();
    private readonly IRepository<DanhMucChucVu, int> DanhMucChucVu = serviceProvider.GetRequiredService<IRepository<DanhMucChucVu, int>>();
    private readonly IRepository<DanhMucLoaiVanBan, int> DanhMucLoaiVanBan = serviceProvider.GetRequiredService<IRepository<DanhMucLoaiVanBan, int>>();
    private readonly IRepository<DanhMucLoaiGoiThau, int> DanhMucLoaiGoiThau = serviceProvider.GetRequiredService<IRepository<DanhMucLoaiGoiThau, int>>();
    private readonly IRepository<DanhMucHinhThucLuaChonNhaThau, int> DanhMucHinhThucLuaChonNhaThau = serviceProvider.GetRequiredService<IRepository<DanhMucHinhThucLuaChonNhaThau, int>>();
    private readonly IRepository<DanhMucLoaiHopDong, int> DanhMucLoaiHopDong = serviceProvider.GetRequiredService<IRepository<DanhMucLoaiHopDong, int>>();
    private readonly IRepository<DanhMucPhuongThucLuaChonNhaThau, int> DanhMucPhuongThucLuaChonNhaThau = serviceProvider.GetRequiredService<IRepository<DanhMucPhuongThucLuaChonNhaThau, int>>();
    private readonly IRepository<DanhMucTinhTrangKhoKhan, int> DanhMucTinhTrangKhoKhan = serviceProvider.GetRequiredService<IRepository<DanhMucTinhTrangKhoKhan, int>>();
    private readonly IRepository<DanhMucNhaThau, Guid> DanhMucNhaThau = serviceProvider.GetRequiredService<IRepository<DanhMucNhaThau, Guid>>();
    private readonly IRepository<DanhMucGiaiDoan, int> DanhMucGiaiDoan = serviceProvider.GetRequiredService<IRepository<DanhMucGiaiDoan, int>>();
    private readonly IRepository<DanhMucMucDoKhoKhan, int> DanhMucMucDoKhoKhan = serviceProvider.GetRequiredService<IRepository<DanhMucMucDoKhoKhan, int>>();
    private readonly IRepository<DanhMucTinhTrangThucHienLcnt, int> DanhMucTinhTrangThucHienLcnt = serviceProvider.GetRequiredService<IRepository<DanhMucTinhTrangThucHienLcnt, int>>();
    private readonly IRepository<DmCapDoCntt, int> DmCapDoCntt = serviceProvider.GetRequiredService<IRepository<DmCapDoCntt, int>>();
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> DanhMucTrangThaiPheDuyet = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
    private readonly IRepository<DanhMucPhuongThucKySo, int> DanhMucPhuongThucKySo = serviceProvider.GetRequiredService<IRepository<DanhMucPhuongThucKySo, int>>();


    public async Task<object> Handle(DanhMucGetQuery request, CancellationToken cancellationToken) {
        return request.DanhMuc switch {
            EDanhMuc.DanhMucBuoc => await Get(request, DanhMucBuoc, cancellationToken),
            EDanhMuc.DanhMucBuocTrangThaiTienDo => await Get(request, DanhMucBuocTrangThaiTienDo, cancellationToken),
            EDanhMuc.DanhMucChuDauTu => await Get(request, DanhMucChuDauTu, cancellationToken),
            EDanhMuc.DanhMucHinhThucDauTu => await Get(request, DanhMucHinhThucDauTu, cancellationToken),
            EDanhMuc.DanhMucHinhThucQuanLy => await Get(request, DanhMucHinhThucQuanLy, cancellationToken),
            EDanhMuc.DanhMucLinhVuc => await Get(request, DanhMucLinhVuc, cancellationToken),
            EDanhMuc.DanhMucLoaiDuAn => await Get(request, DanhMucLoaiDuAn, cancellationToken),
            EDanhMuc.DanhMucLoaiDuAnTheoNam => await Get(request, DanhMucLoaiDuAnTheoNam, cancellationToken),
            EDanhMuc.DanhMucNguonVon => await Get(request, DanhMucNguonVon, cancellationToken),
            EDanhMuc.DanhMucNhomDuAn => await Get(request, DanhMucNhomDuAn, cancellationToken),
            EDanhMuc.DanhMucQuyTrinh => await Get(request, DanhMucQuyTrinh, cancellationToken),
            EDanhMuc.DanhMucTrangThaiTienDo => await Get(request, DanhMucTrangThaiTienDo, cancellationToken),
            EDanhMuc.DanhMucTrangThaiDuAn => await Get(request, DanhMucTrangThaiDuAn, cancellationToken),
            EDanhMuc.DanhMucManHinh => await Get(request, DanhMucManHinh, cancellationToken),
            EDanhMuc.DanhMucChucVu => await Get(request, DanhMucChucVu, cancellationToken),
            EDanhMuc.DanhMucLoaiVanBan => await Get(request, DanhMucLoaiVanBan, cancellationToken),
            EDanhMuc.DanhMucHinhThucLuaChonNhaThau => await Get(request, DanhMucHinhThucLuaChonNhaThau,
                cancellationToken),
            EDanhMuc.DanhMucLoaiHopDong => await Get(request, DanhMucLoaiHopDong, cancellationToken),
            EDanhMuc.DanhMucPhuongThucLuaChonNhaThau =>
                await Get(request, DanhMucPhuongThucLuaChonNhaThau, cancellationToken),
            EDanhMuc.DanhMucLoaiGoiThau => await Get(request, DanhMucLoaiGoiThau, cancellationToken),
            EDanhMuc.DanhMucTinhTrangKhoKhan => await Get(request, DanhMucTinhTrangKhoKhan, cancellationToken),
            EDanhMuc.DanhMucNhaThau => await Get(request, DanhMucNhaThau, cancellationToken),
            EDanhMuc.DanhMucGiaiDoan => await Get(request, DanhMucGiaiDoan, cancellationToken),
            EDanhMuc.DanhMucMucDoKhoKhan => await Get(request, DanhMucMucDoKhoKhan, cancellationToken),
            EDanhMuc.DanhMucTinhTrangThucHienLcnt => await Get(request, DanhMucTinhTrangThucHienLcnt, cancellationToken),
            EDanhMuc.DmCapDoCntt => await Get(request, DmCapDoCntt, cancellationToken),
            EDanhMuc.DanhMucPhuongThucKySo => await Get(request, DanhMucPhuongThucKySo, cancellationToken),
            EDanhMuc.DanhMucTrangThaiPheDuyet => await Get(request, DanhMucTrangThaiPheDuyet, cancellationToken),
            _ => throw new ManagedException(),
        };
    }

    private static async Task<TEntity> Get<TEntity, TKey>(DanhMucGetQuery request,
        IRepository<TEntity, TKey> repo,
        CancellationToken cancellationToken = default)
        where TEntity : class, IHasKey<TKey>, IAggregateRoot, new()
        where TKey : notnull {
        var query = repo.GetOrderedSet();
        // var query = request is { Enum: true } ? repo.GetOriginalSet() : repo.GetQueryableSet();


        var entity = await query.FirstOrDefaultAsync(o => o.Id!.ToString() == request.Id, cancellationToken);
        ManagedException.ThrowIf(request.ThrowIfNull && entity == null, "Không tìm thấy dữ liệu");
        return entity!;
    }
}