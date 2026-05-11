using Microsoft.EntityFrameworkCore;
using QLDA.Application.QuanLyPheDuyet.DTOs;
using QLDA.Domain.Constants;

namespace QLDA.Application.QuanLyPheDuyet.Queries;

/// <summary>
/// Chi tiet ban ghi pheduyet theo type + id
/// </summary>
public record PheDuyetGetChiTietQuery : IRequest<PheDuyetChiTietDto> {
    public string Type { get; set; } = default!;
    public Guid Id { get; set; }
    public bool IsNoTracking { get; set; }
}

internal class PheDuyetGetChiTietQueryHandler : IRequestHandler<PheDuyetGetChiTietQuery, PheDuyetChiTietDto> {
    private readonly IRepository<PheDuyetDuToan, Guid> _duToanRepo;
    private readonly IRepository<HoSoDeXuatCapDoCntt, Guid> _hoSoCnttRepo;
    private readonly IRepository<HoSoMoiThauDienTu, Guid> _hoSoThauRepo;
    private readonly IRepository<PheDuyetHistory, Guid> _historyRepo;
    private readonly IServiceProvider _serviceProvider;

    public PheDuyetGetChiTietQueryHandler(IServiceProvider serviceProvider) {
        _serviceProvider = serviceProvider;
        _duToanRepo = serviceProvider.GetRequiredService<IRepository<PheDuyetDuToan, Guid>>();
        _hoSoCnttRepo = serviceProvider.GetRequiredService<IRepository<HoSoDeXuatCapDoCntt, Guid>>();
        _hoSoThauRepo = serviceProvider.GetRequiredService<IRepository<HoSoMoiThauDienTu, Guid>>();
        _historyRepo = serviceProvider.GetRequiredService<IRepository<PheDuyetHistory, Guid>>();
    }

    public async Task<PheDuyetChiTietDto> Handle(PheDuyetGetChiTietQuery request, CancellationToken cancellationToken) {
        object? entity = request.Type switch {
            PheDuyetEntityNames.PheDuyetDuToan => await GetDuToanDetail(request.Id, cancellationToken),
            PheDuyetEntityNames.HoSoDeXuatCapDoCntt => await GetHoSoDeXuatCapDoCnttDetail(request.Id, cancellationToken),
            PheDuyetEntityNames.HoSoMoiThauDienTu => await GetHoSoMoiThauDienTuDetail(request.Id, cancellationToken),
            _ => throw new ManagedException($"Loại phê duyệt '{request.Type}' không hợp lệ")
        };

        ManagedException.ThrowIfNull(entity, "Không tìm thấy bản ghi phê duyệt");

        var lichSu = (await _historyRepo.GetQueryableSet()
            .Include(h => h.TrangThai)
            .Where(h => h.EntityId == request.Id && h.EntityName == request.Type)
            .Select(h => new PheDuyetHistoryDto {
                Id = h.Id,
                EntityName = h.EntityName,
                EntityId = h.EntityId,
                DuAnId = h.DuAnId,
                NguoiXuLyId = h.NguoiXuLyId,
                TrangThaiId = h.TrangThaiId,
                MaTrangThai = h.TrangThai != null ? h.TrangThai.Ma : null,
                TenTrangThai = h.TrangThai != null ? h.TrangThai.Ten : null,
                NoiDung = h.NoiDung,
                NgayXuLy = h.NgayXuLy
            })
            .ToListAsync(cancellationToken))
            .OrderByDescending(h => h.NgayXuLy)
            .ToList();

        return new PheDuyetChiTietDto {
            Type = request.Type,
            Id = request.Id,
            Entity = entity,
            LichSu = lichSu
        };
    }

    private async Task<object?> GetDuToanDetail(Guid id, CancellationToken cancellationToken) {
        return await _duToanRepo.GetQueryableSet()
            .Include(e => e.TrangThai)
            .Include(e => e.ChucVu)
            .Where(e => e.Id == id)
            .Select(e => new {
                e.Id, e.DuAnId, e.BuocId, SoVanBan = e.So, e.NgayKy, e.NguoiKy,
                e.ChucVuId, e.GiaTriDuThau, e.TrichYeu, e.TrangThaiId,
                MaTrangThai = e.TrangThai != null ? e.TrangThai.Ma : null,
                TenTrangThai = e.TrangThai != null ? e.TrangThai.Ten : null,
                e.NguoiXuLyId, e.NguoiGiaoViecId,
                TenChucVu = e.ChucVu != null ? e.ChucVu.Ten : null
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<object?> GetHoSoDeXuatCapDoCnttDetail(Guid id, CancellationToken cancellationToken) {
        return await _hoSoCnttRepo.GetQueryableSet()
            .Include(e => e.TrangThai)
            .Include(e => e.CapDo)
            .Where(e => e.Id == id)
            .Select(e => new {
                e.Id, e.DuAnId, e.BuocId, e.TrangThaiId, e.CapDoId,
                e.NgayTrinh, e.DonViChuTriId,
                e.NoiDungDeNghi, e.NoiDungBaoCao, e.NoiDungDuThao,
                MaTrangThai = e.TrangThai != null ? e.TrangThai.Ma : null,
                TenTrangThai = e.TrangThai != null ? e.TrangThai.Ten : null,
                TenCapDo = e.CapDo != null ? e.CapDo.Ten : null
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<object?> GetHoSoMoiThauDienTuDetail(Guid id, CancellationToken cancellationToken) {
        return await _hoSoThauRepo.GetQueryableSet()
            .Include(e => e.TrangThaiPheDuyet)
            .Include(e => e.HinhThucLuaChonNhaThau)
            .Where(e => e.Id == id)
            .Select(e => new {
                e.Id, e.DuAnId, e.BuocId, e.TrangThaiId,
                e.HinhThucLuaChonNhaThauId, e.GoiThauId,
                e.GiaTri, e.ThoiGianThucHien, e.TrangThaiDangTai,
                MaTrangThai = e.TrangThaiPheDuyet != null ? e.TrangThaiPheDuyet.Ma : null,
                TenTrangThai = e.TrangThaiPheDuyet != null ? e.TrangThaiPheDuyet.Ten : null,
                TenHinhThuc = e.HinhThucLuaChonNhaThau != null ? e.HinhThucLuaChonNhaThau.Ten : null
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
