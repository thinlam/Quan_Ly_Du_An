using Microsoft.EntityFrameworkCore;
using QLDA.Application.QuanLyPheDuyet.DTOs;
using QLDA.Domain.Constants;

namespace QLDA.Application.QuanLyPheDuyet.Queries;

/// <summary>
/// Chi tiet ban ghi pheduyet theo type + id
/// </summary>
public record PheDuyetGetChiTietQuery : IRequest<PheDuyetChiTietDto>
{
    public string Type { get; set; } = default!;
    public Guid Id { get; set; }
    public bool IsNoTracking { get; set; }
}

internal class PheDuyetGetChiTietQueryHandler : IRequestHandler<PheDuyetGetChiTietQuery, PheDuyetChiTietDto>
{
    private readonly IRepository<PheDuyetDuToan, Guid> _duToanRepo;
    private readonly IRepository<HoSoDeXuatCapDoCntt, Guid> _hoSoCnttRepo;
    private readonly IRepository<DeXuatNhuCauKinhPhi, Guid> _dxNhuCauKinhPhi;
    private readonly IRepository<HoSoMoiThauDienTu, Guid> _hoSoThauRepo;
    private readonly IRepository<DeXuatChuTruongMoi, Guid> _dxChuTruongMoi;
    private readonly IRepository<DeXuatChuyenTiep, Guid> _dxChuyenTiep;
    private readonly IRepository<BaoCaoKetQuaKhaoSat, Guid> _baoCaoKhaoSatRepo;
    private readonly IRepository<DuToanDauTu, Guid> _duToanDauTuRepo;
    private readonly IRepository<PheDuyetHistory, Guid> _historyRepo;
    private readonly IRepository<ToTrinhThamDinhNhaThau, Guid> _toTrinhThamDinhRepo;
    private readonly IRepository<ToTrinhPheDuyet, Guid> _toTrinhPheDuyet;
    private readonly IRepository<KeHoachLuaChonNhaThauRutGon, Guid> _keHoachNhaThauRutGon;
    private readonly IRepository<KeHoachTrienKhaiHangMuc, Guid> _trienKhaiHangMuc;
    private readonly IRepository<QuyetDinhDieuChinh, Guid> _quyetDinhDieuChiRepo;
    private readonly IRepository<ChuTruongLapKeHoach, Guid> _chuTruongLapKeHoach;
    private readonly IRepository<ThoaThuanGiaoViec, Guid> _ThoaThuanGiaoViec;
    private readonly IRepository<ToTrinhKetQuaGoiThau, Guid> _ToTrinhKetQuaGoiThau;

    private readonly IServiceProvider _serviceProvider;

    public PheDuyetGetChiTietQueryHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _quyetDinhDieuChiRepo = serviceProvider.GetRequiredService<IRepository<QuyetDinhDieuChinh, Guid>>();
        _duToanRepo = serviceProvider.GetRequiredService<IRepository<PheDuyetDuToan, Guid>>();
        _trienKhaiHangMuc = serviceProvider.GetRequiredService<IRepository<KeHoachTrienKhaiHangMuc, Guid>>();
        _duToanDauTuRepo = serviceProvider.GetRequiredService<IRepository<DuToanDauTu, Guid>>();
        _dxNhuCauKinhPhi = serviceProvider.GetRequiredService<IRepository<DeXuatNhuCauKinhPhi, Guid>>();
        _dxChuTruongMoi = serviceProvider.GetRequiredService<IRepository<DeXuatChuTruongMoi, Guid>>();
        _toTrinhThamDinhRepo = serviceProvider.GetRequiredService<IRepository<ToTrinhThamDinhNhaThau, Guid>>();
        _toTrinhPheDuyet = serviceProvider.GetRequiredService<IRepository<ToTrinhPheDuyet, Guid>>();
        _dxChuyenTiep = serviceProvider.GetRequiredService<IRepository<DeXuatChuyenTiep, Guid>>();
        _hoSoCnttRepo = serviceProvider.GetRequiredService<IRepository<HoSoDeXuatCapDoCntt, Guid>>();
        _hoSoThauRepo = serviceProvider.GetRequiredService<IRepository<HoSoMoiThauDienTu, Guid>>();
        _baoCaoKhaoSatRepo = serviceProvider.GetRequiredService<IRepository<BaoCaoKetQuaKhaoSat, Guid>>();
        _keHoachNhaThauRutGon = serviceProvider.GetRequiredService<IRepository<KeHoachLuaChonNhaThauRutGon, Guid>>();
        _historyRepo = serviceProvider.GetRequiredService<IRepository<PheDuyetHistory, Guid>>();
        _chuTruongLapKeHoach = serviceProvider.GetRequiredService<IRepository<ChuTruongLapKeHoach, Guid>>();
        _ThoaThuanGiaoViec = serviceProvider.GetRequiredService<IRepository<ThoaThuanGiaoViec, Guid>>();
        _ToTrinhKetQuaGoiThau = serviceProvider.GetRequiredService<IRepository<ToTrinhKetQuaGoiThau, Guid>>();
    }

    public async Task<PheDuyetChiTietDto> Handle(PheDuyetGetChiTietQuery request, CancellationToken cancellationToken)
    {
        object? entity = request.Type switch
        {
            PheDuyetEntityNames.PheDuyetDuToan => await GetDuToanDetail(request.Id, cancellationToken),
            PheDuyetEntityNames.HoSoDeXuatCapDoCntt => await GetHoSoDeXuatCapDoCnttDetail(request.Id, cancellationToken),
            PheDuyetEntityNames.HoSoMoiThauDienTu => await GetHoSoMoiThauDienTuDetail(request.Id, cancellationToken),
            PheDuyetEntityNames.BaoCaoKetQuaKhaoSat => await GetBaoCaoKetQuaKhaoSatDetail(request.Id, cancellationToken),
            PheDuyetEntityNames.DeXuatNhuCauKinhPhi => await GetDeXuatNhuCauKinhPhiDetail(request.Id, cancellationToken),
            PheDuyetEntityNames.KeHoachLuaChonNhaThauRutGon => await GetKeHoachLuaChonNhaThauRutGonDetail(request.Id, cancellationToken),
            PheDuyetEntityNames.KeHoachTrienKhaiHangMuc => await GetKeHoachTrienKhaiHangMucDetail(request.Id, cancellationToken),
            PheDuyetEntityNames.QuyetDinhDieuChinh => await GeQuyetDinhDieuChinhDetail(request.Id, cancellationToken),
            //  PheDuyetEntityNames.DeXuatChuTruongMoi => await GetDeXuatNhuCauKinhPhiDetail(request.Id, cancellationToken),
            //  PheDuyetEntityNames.DeXuatChuTruongChuyenTiep => await GetDeXuatNhuCauKinhPhiDetail(request.Id, cancellationToken),
            PheDuyetEntityNames.DuToanDauTu => await GetDuToanDauTuDetail(request.Id, cancellationToken),
            PheDuyetEntityNames.ToTrinhThamDinhNhaThau => await GetToTrinhThamDinhNhaThauDetail(request.Id, cancellationToken),
            PheDuyetEntityNames.ChuTruongLapKeHoach => await GetChuTruongLapKeHoachDetail(request.Id, cancellationToken),

            PheDuyetEntityNames.KHLCNTDuToanSanCo => await GetToTrinhPheDuyetDetail(request.Id, cancellationToken),
            PheDuyetEntityNames.QuyetDinhKeHoachThue => await GetToTrinhPheDuyetDetail(request.Id, cancellationToken),
            PheDuyetEntityNames.KHLCNTDuToanYeuCauRieng => await GetToTrinhPheDuyetDetail(request.Id, cancellationToken),
            PheDuyetEntityNames.KeHoachLCNTChuanBiDauTu => await GetToTrinhPheDuyetDetail(request.Id, cancellationToken),
            PheDuyetEntityNames.PheDuyetKhaoSat => await GetToTrinhPheDuyetDetail(request.Id, cancellationToken),
            PheDuyetEntityNames.KeHoachTongTheLCNT => await GetToTrinhPheDuyetDetail(request.Id, cancellationToken),

            PheDuyetEntityNames.ThoaThuanGiaoViec => await GetThoaThuanGiaoViecDetail(request.Id, cancellationToken),
            PheDuyetEntityNames.ToTrinhKetQuaGoiThau => await GetToTrinhKetQuaGoiThauDetail(request.Id, cancellationToken),
            //  PheDuyetEntityNames. => await Get Detail(request.Id, cancellationToken),
            _ => throw new ManagedException($"Loại phê duyệt '{request.Type}' không hợp lệ")
        };

        ManagedException.ThrowIfNull(entity, "Không tìm thấy bản ghi phê duyệt");

        var lichSu = (await _historyRepo.GetQueryableSet()
            .Include(h => h.TrangThai)
            .Where(h => h.EntityId == request.Id && h.EntityName == request.Type)
            .Select(h => new PheDuyetHistoryDto
            {
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

        return new PheDuyetChiTietDto
        {
            Type = request.Type,
            Id = request.Id,
            Entity = entity,
            LichSu = lichSu
        };
    }
    private async Task<object?> GetToTrinhKetQuaGoiThauDetail(Guid id, CancellationToken cancellationToken)
    {
        return await _ToTrinhKetQuaGoiThau.GetQueryableSet()
            .Include(e => e.TrangThai)
            .Include(e => e.DuAn).ThenInclude(x => x.BuocHienTai)
            .Include(e => e.DuAn).ThenInclude(x => x.GiaiDoanHienTai).Include(e => e.DuAn)
            .Where(e => e.Id == id)
            .Select(e => new
            {
                e.Id,
                e.DuAnId,
                e.BuocId,
                e.TrangThaiId,
                TenDuAn = e.DuAn != null ? e.DuAn.TenDuAn : "",
                TenGiaiDoan = e.DuAn != null && e.DuAn.GiaiDoanHienTai != null ? e.DuAn.GiaiDoanHienTai.Ten : "",
                TenBuoc = e.DuAn != null && e.DuAn.BuocHienTai != null ? e.DuAn.BuocHienTai.TenBuoc : "",

                MaTrangThai = e.TrangThai != null && e.TrangThai.Ma != "LEG" ? e.TrangThai.Ma : TrangThaiPheDuyetCodes.Default.DuThao,
                TenTrangThai = e.TrangThai != null && e.TrangThai.Ma != "LEG" ? e.TrangThai.Ten : TrangThaiPheDuyetCodes.Default.TenDuThao,
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
    private async Task<object?> GetThoaThuanGiaoViecDetail(Guid id, CancellationToken cancellationToken)
    {
        return await _ThoaThuanGiaoViec.GetQueryableSet()
            .Include(e => e.TrangThai)
            .Include(e => e.DuAn).ThenInclude(x => x.BuocHienTai)
            .Include(e => e.DuAn).ThenInclude(x => x.GiaiDoanHienTai).Include(e => e.DuAn)
            .Where(e => e.Id == id)
            .Select(e => new
            {
                e.Id,
                e.DuAnId,
                e.BuocId,
                e.TrangThaiId,
                TenDuAn = e.DuAn != null ? e.DuAn.TenDuAn : "",
                TenGiaiDoan = e.DuAn != null && e.DuAn.GiaiDoanHienTai != null ? e.DuAn.GiaiDoanHienTai.Ten : "",
                TenBuoc = e.DuAn != null && e.DuAn.BuocHienTai != null ? e.DuAn.BuocHienTai.TenBuoc : "",

                MaTrangThai = e.TrangThai != null && e.TrangThai.Ma != "LEG" ? e.TrangThai.Ma : TrangThaiPheDuyetCodes.Default.DuThao,
                TenTrangThai = e.TrangThai != null && e.TrangThai.Ma != "LEG" ? e.TrangThai.Ten : TrangThaiPheDuyetCodes.Default.TenDuThao,
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<object?> GetQuyetDinhKeHoachThueDetail(Guid id, CancellationToken cancellationToken)
    {
        return await _chuTruongLapKeHoach.GetQueryableSet()
            .Include(e => e.TrangThai)
            .Include(e => e.DuAn).ThenInclude(x => x.BuocHienTai)
            .Include(e => e.DuAn).ThenInclude(x => x.GiaiDoanHienTai).Include(e => e.DuAn)
            .Where(e => e.Id == id)
            .Select(e => new
            {
                e.Id,
                e.DuAnId,
                e.BuocId,
                e.TrangThaiId,
                TenDuAn = e.DuAn != null ? e.DuAn.TenDuAn : "",
                TenGiaiDoan = e.DuAn != null && e.DuAn.GiaiDoanHienTai != null ? e.DuAn.GiaiDoanHienTai.Ten : "",
                TenBuoc = e.DuAn != null && e.DuAn.BuocHienTai != null ? e.DuAn.BuocHienTai.TenBuoc : "",

                MaTrangThai = e.TrangThai != null && e.TrangThai.Ma != "LEG" ? e.TrangThai.Ma : TrangThaiPheDuyetCodes.Default.DuThao,
                TenTrangThai = e.TrangThai != null && e.TrangThai.Ma != "LEG" ? e.TrangThai.Ten : TrangThaiPheDuyetCodes.Default.TenDuThao,
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
    private async Task<object?> GetChuTruongLapKeHoachDetail(Guid id, CancellationToken cancellationToken)
    {
        return await _chuTruongLapKeHoach.GetQueryableSet()
            .Include(e => e.TrangThai)
            .Include(e => e.DuAn).ThenInclude(x => x.BuocHienTai)
            .Include(e => e.DuAn).ThenInclude(x => x.GiaiDoanHienTai).Include(e => e.DuAn)
            .Where(e => e.Id == id)
            .Select(e => new
            {
                e.Id,
                e.DuAnId,
                e.BuocId,
                e.TrangThaiId,
                TenDuAn = e.DuAn != null ? e.DuAn.TenDuAn : "",
                TenGiaiDoan = e.DuAn != null && e.DuAn.GiaiDoanHienTai != null ? e.DuAn.GiaiDoanHienTai.Ten : "",
                TenBuoc = e.DuAn != null && e.DuAn.BuocHienTai != null ? e.DuAn.BuocHienTai.TenBuoc : "",

                MaTrangThai = e.TrangThai != null && e.TrangThai.Ma != "LEG" ? e.TrangThai.Ma : TrangThaiPheDuyetCodes.Default.DuThao,
                TenTrangThai = e.TrangThai != null && e.TrangThai.Ma != "LEG" ? e.TrangThai.Ten : TrangThaiPheDuyetCodes.Default.TenDuThao,
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
    private async Task<object?> GeQuyetDinhDieuChinhDetail(Guid id, CancellationToken cancellationToken)
    {
        return await _quyetDinhDieuChiRepo.GetQueryableSet()
            .Include(e => e.TrangThai)
            .Include(e => e.DuAn).ThenInclude(x => x.BuocHienTai)
            .Include(e => e.DuAn).ThenInclude(x => x.GiaiDoanHienTai).Include(e => e.DuAn)
            .Where(e => e.Id == id)
            .Select(e => new
            {
                e.Id,
                e.DuAnId,
                e.BuocId,
                e.TrangThaiId,
                TenDuAn = e.DuAn != null ? e.DuAn.TenDuAn : "",
                TenGiaiDoan = e.DuAn != null && e.DuAn.GiaiDoanHienTai != null ? e.DuAn.GiaiDoanHienTai.Ten : "",
                TenBuoc = e.DuAn != null && e.DuAn.BuocHienTai != null ? e.DuAn.BuocHienTai.TenBuoc : "",

                MaTrangThai = e.TrangThai != null && e.TrangThai.Ma != "LEG" ? e.TrangThai.Ma : TrangThaiPheDuyetCodes.Default.DuThao,
                TenTrangThai = e.TrangThai != null && e.TrangThai.Ma != "LEG" ? e.TrangThai.Ten : TrangThaiPheDuyetCodes.Default.TenDuThao,
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<object?> GetKeHoachTrienKhaiHangMucDetail(Guid id, CancellationToken cancellationToken)
    {
        return await _trienKhaiHangMuc.GetQueryableSet()
            .Include(e => e.TrangThai)
            .Include(e => e.DuAn).ThenInclude(x => x.BuocHienTai)
            .Include(e => e.DuAn).ThenInclude(x => x.GiaiDoanHienTai).Include(e => e.DuAn)
            .Where(e => e.Id == id)
            .Select(e => new
            {
                e.Id,
                e.DuAnId,
                e.BuocId,
                e.TrangThaiId,
                TenDuAn = e.DuAn != null ? e.DuAn.TenDuAn : "",
                TenGiaiDoan = e.DuAn != null && e.DuAn.GiaiDoanHienTai != null ? e.DuAn.GiaiDoanHienTai.Ten : "",
                TenBuoc = e.DuAn != null && e.DuAn.BuocHienTai != null ? e.DuAn.BuocHienTai.TenBuoc : "",

                MaTrangThai = e.TrangThai != null && e.TrangThai.Ma != "LEG" ? e.TrangThai.Ma : TrangThaiPheDuyetCodes.Default.DuThao,
                TenTrangThai = e.TrangThai != null && e.TrangThai.Ma != "LEG" ? e.TrangThai.Ten : TrangThaiPheDuyetCodes.Default.TenDuThao,
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
    private async Task<object?> GetKeHoachLuaChonNhaThauRutGonDetail(Guid id, CancellationToken cancellationToken)
    {
        return await _keHoachNhaThauRutGon.GetQueryableSet()
            .Include(e => e.TrangThai)

            .Include(e => e.DuAn).ThenInclude(x => x.BuocHienTai)
            .Include(e => e.DuAn).ThenInclude(x => x.GiaiDoanHienTai).Include(e => e.DuAn)
            .Where(e => e.Id == id)
            .Select(e => new
            {
                e.Id,
                e.DuAnId,
                e.BuocId,
                e.TrangThaiId,
                TenDuAn = e.DuAn != null ? e.DuAn.TenDuAn : "",
                TenGiaiDoan = e.DuAn != null && e.DuAn.GiaiDoanHienTai != null ? e.DuAn.GiaiDoanHienTai.Ten : "",
                TenBuoc = e.DuAn != null && e.DuAn.BuocHienTai != null ? e.DuAn.BuocHienTai.TenBuoc : "",

                MaTrangThai = e.TrangThai != null && e.TrangThai.Ma != "LEG" ? e.TrangThai.Ma : TrangThaiPheDuyetCodes.Default.DuThao,
                TenTrangThai = e.TrangThai != null && e.TrangThai.Ma != "LEG" ? e.TrangThai.Ten : TrangThaiPheDuyetCodes.Default.TenDuThao,
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
    private async Task<object?> GetDuToanDetail(Guid id, CancellationToken cancellationToken)
    {
        return await _duToanRepo.GetQueryableSet()
            .Include(e => e.TrangThai)
            .Include(e => e.ChucVu)
            .Where(e => e.Id == id)
            .Select(e => new
            {
                e.Id,
                e.DuAnId,
                e.BuocId,
                SoVanBan = e.So,
                e.NgayKy,
                e.NguoiKy,
                e.ChucVuId,
                e.GiaTriDuThau,
                e.TrichYeu,
                e.TrangThaiId,
                MaTrangThai = e.TrangThai != null && e.TrangThai.Ma != "LEG" ? e.TrangThai.Ma : TrangThaiPheDuyetCodes.Default.DuThao,
                TenTrangThai = e.TrangThai != null && e.TrangThai.Ma != "LEG" ? e.TrangThai.Ten : TrangThaiPheDuyetCodes.Default.TenDuThao,
                e.NguoiXuLyId,
                TenChucVu = e.ChucVu != null ? e.ChucVu.Ten : null
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
    private async Task<object?> GetToTrinhPheDuyetDetail(Guid id, CancellationToken cancellationToken)
    {
        return await _toTrinhPheDuyet.GetQueryableSet()
            .Include(e => e.TrangThai)
            .Include(e => e.DuAn).ThenInclude(x => x.BuocHienTai)
            .Include(e => e.DuAn).ThenInclude(x => x.GiaiDoanHienTai).Include(e => e.DuAn)
            .Where(e => e.Id == id)
            .Select(e => new
            {
                e.Id,
                e.DuAnId,
                e.BuocId,
                e.TrangThaiId,
                TenDuAn = e.DuAn != null ? e.DuAn.TenDuAn : "",
                TenGiaiDoan = e.DuAn != null && e.DuAn.GiaiDoanHienTai != null ? e.DuAn.GiaiDoanHienTai.Ten : "",
                TenBuoc = e.DuAn != null && e.DuAn.BuocHienTai != null ? e.DuAn.BuocHienTai.TenBuoc : "",

                MaTrangThai = e.TrangThai != null && e.TrangThai.Ma != "LEG" ? e.TrangThai.Ma : TrangThaiPheDuyetCodes.Default.DuThao,
                TenTrangThai = e.TrangThai != null && e.TrangThai.Ma != "LEG" ? e.TrangThai.Ten : TrangThaiPheDuyetCodes.Default.TenDuThao,
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
    private async Task<object?> GetToTrinhThamDinhNhaThauDetail(Guid id, CancellationToken cancellationToken)
    {
        return await _toTrinhThamDinhRepo.GetQueryableSet()
              .Include(e => e.DuAn).ThenInclude(x => x.BuocHienTai)
            .Include(e => e.DuAn).ThenInclude(x => x.GiaiDoanHienTai).Include(e => e.DuAn)
            .Include(e => e.TrangThai)
            .Where(e => e.Id == id)
            .Select(e => new
            {
                e.Id,
                e.DuAnId,
                e.BuocId,
                e.TrangThaiId,
                TenDuAn = e.DuAn != null ? e.DuAn.TenDuAn : "",
                TenGiaiDoan = e.DuAn != null && e.DuAn.GiaiDoanHienTai != null ? e.DuAn.GiaiDoanHienTai.Ten : "",
                TenBuoc = e.DuAn != null && e.DuAn.BuocHienTai != null ? e.DuAn.BuocHienTai.TenBuoc : "",

                MaTrangThai = e.TrangThai != null && e.TrangThai.Ma != "LEG" ? e.TrangThai.Ma : TrangThaiPheDuyetCodes.Default.DuThao,
                TenTrangThai = e.TrangThai != null && e.TrangThai.Ma != "LEG" ? e.TrangThai.Ten : TrangThaiPheDuyetCodes.Default.TenDuThao,
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
    private async Task<object?> GetDuToanDauTuDetail(Guid id, CancellationToken cancellationToken)
    {
        return await _duToanDauTuRepo.GetQueryableSet()
            .Include(e => e.TrangThai)
            .Include(e => e.DuAn).ThenInclude(x => x.BuocHienTai)
            .Include(e => e.DuAn).ThenInclude(x => x.GiaiDoanHienTai)
            .Where(e => e.Id == id && !e.IsDeleted)
            .Where(e => e.Id == id)
            .Select(e => new
            {
                e.Id,
                e.DuAnId,
                e.BuocId,
                e.TrangThaiId,
                TenDuAn = e.DuAn != null ? e.DuAn.TenDuAn : "",
                TenGiaiDoan = e.DuAn != null && e.DuAn.GiaiDoanHienTai != null ? e.DuAn.GiaiDoanHienTai.Ten : "",
                TenBuoc = e.DuAn != null && e.DuAn.BuocHienTai != null ? e.DuAn.BuocHienTai.TenBuoc : "",

                MaTrangThai = e.TrangThai != null && e.TrangThai.Ma != "LEG" ? e.TrangThai.Ma : TrangThaiPheDuyetCodes.Default.DuThao,
                TenTrangThai = e.TrangThai != null && e.TrangThai.Ma != "LEG" ? e.TrangThai.Ten : TrangThaiPheDuyetCodes.Default.TenDuThao,
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
    private async Task<object?> GetHoSoDeXuatCapDoCnttDetail(Guid id, CancellationToken cancellationToken)
    {
        return await _hoSoCnttRepo.GetQueryableSet()
            .Include(e => e.TrangThai)
            .Include(e => e.CapDo)
            .Where(e => e.Id == id)
            .Select(e => new
            {
                e.Id,
                e.DuAnId,
                e.BuocId,
                e.TrangThaiId,
                e.CapDoId,
                e.NgayTrinh,
                e.DonViChuTriId,
                e.NoiDungDeNghi,
                e.NoiDungBaoCao,
                e.NoiDungDuThao,
                MaTrangThai = e.TrangThai != null && e.TrangThai.Ma != "LEG" ? e.TrangThai.Ma : TrangThaiPheDuyetCodes.Default.DuThao,
                TenTrangThai = e.TrangThai != null && e.TrangThai.Ma != "LEG" ? e.TrangThai.Ten : TrangThaiPheDuyetCodes.Default.TenDuThao,
                TenCapDo = e.CapDo != null ? e.CapDo.Ten : null
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<object?> GetHoSoMoiThauDienTuDetail(Guid id, CancellationToken cancellationToken)
    {
        return await _hoSoThauRepo.GetQueryableSet()
            .Include(e => e.TrangThaiPheDuyet)
            .Include(e => e.HinhThucLuaChonNhaThau)
            .Where(e => e.Id == id)
            .Select(e => new
            {
                e.Id,
                e.DuAnId,
                e.BuocId,
                e.TrangThaiId,
                e.HinhThucLuaChonNhaThauId,
                e.GoiThauId,
                e.GiaTri,
                e.ThoiGianThucHien,
                e.TrangThaiDangTai,
                MaTrangThai = e.TrangThaiPheDuyet != null && e.TrangThaiPheDuyet.Ma != "LEG" ? e.TrangThaiPheDuyet.Ma : TrangThaiPheDuyetCodes.Default.DuThao,
                TenTrangThai = e.TrangThaiPheDuyet != null && e.TrangThaiPheDuyet.Ma != "LEG" ? e.TrangThaiPheDuyet.Ten : TrangThaiPheDuyetCodes.Default.TenDuThao,
                TenHinhThuc = e.HinhThucLuaChonNhaThau != null ? e.HinhThucLuaChonNhaThau.Ten : null
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
    private async Task<object?> GetDeXuatNhuCauKinhPhiDetail(Guid id, CancellationToken cancellationToken)
    {
        return await _dxNhuCauKinhPhi.GetQueryableSet()
            .Include(e => e.TrangThai)
            .Include(e => e.DuAn).ThenInclude(x => x.BuocHienTai)
            .Include(e => e.DuAn).ThenInclude(x => x.GiaiDoanHienTai)
            .Where(e => e.Id == id && !e.IsDeleted)
            .Select(e => new
            {
                e.Id,
                e.DuAnId,
                e.BuocId,
                e.TrangThaiId,
                TenDuAn = e.DuAn != null ? e.DuAn.TenDuAn : "",
                TenGiaiDoan = e.DuAn != null && e.DuAn.GiaiDoanHienTai != null ? e.DuAn.GiaiDoanHienTai.Ten : "",
                TenBuoc = e.DuAn != null && e.DuAn.BuocHienTai != null ? e.DuAn.BuocHienTai.TenBuoc : "",

                MaTrangThai = e.TrangThai != null && e.TrangThai.Ma != "LEG" ? e.TrangThai.Ma : TrangThaiPheDuyetCodes.Default.DuThao,
                TenTrangThai = e.TrangThai != null && e.TrangThai.Ma != "LEG" ? e.TrangThai.Ten : TrangThaiPheDuyetCodes.Default.TenDuThao,
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
    private async Task<object?> GetBaoCaoKetQuaKhaoSatDetail(Guid id, CancellationToken cancellationToken)
    {
        return await _baoCaoKhaoSatRepo.GetQueryableSet()
            .Include(e => e.TrangThai)
            .Include(e => e.DuAn).ThenInclude(x => x.BuocHienTai)
            .Include(e => e.DuAn).ThenInclude(x => x.GiaiDoanHienTai).Where(e => e.Id == id && !e.IsDeleted)
            .Select(e => new
            {
                e.Id,
                e.DuAnId,
                e.BuocId,
                e.TrangThaiId,
                e.NgayTrinh,
                TenDuAn = e.DuAn != null ? e.DuAn.TenDuAn : "",
                TenGiaiDoan = e.DuAn != null && e.DuAn.GiaiDoanHienTai != null ? e.DuAn.GiaiDoanHienTai.Ten : "",
                TenBuoc = e.DuAn != null && e.DuAn.BuocHienTai != null ? e.DuAn.BuocHienTai.TenBuoc : "",

                e.NoiDungBaoCao,
                e.NoiDungNghiemThu,
                NgayKhaoSat = e.NgayKhaoSat,
                MaTrangThai = e.TrangThai != null && e.TrangThai.Ma != "LEG" ? e.TrangThai.Ma : TrangThaiPheDuyetCodes.Default.DuThao,
                TenTrangThai = e.TrangThai != null && e.TrangThai.Ma != "LEG" ? e.TrangThai.Ten : TrangThaiPheDuyetCodes.Default.TenDuThao,
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
