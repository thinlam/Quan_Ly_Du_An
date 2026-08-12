using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.ToTrinhThamDinhNhaThaus.DTOs;
using QLDA.Domain.Constants;
using QLDA.Domain.Enums;

namespace QLDA.Application.ToTrinhThamDinhNhaThaus.Commands;

/// <summary>
/// Kết quả tạo mới — trả thêm Id của ToTrinhQuyetDinh/VanBanQuyetDinh để Controller
/// lưu TepDinhKem đúng GroupId cho từng mục (Issue #179).
/// </summary>
public record ToTrinhThamDinhNhaThauThemMoiResult(
    ToTrinhThamDinhNhaThau Entity,
    long? ToTrinhQuyetDinhId,
    Guid? VanBanQuyetDinhId);

public record ToTrinhThamDinhNhaThauThemMoiCommand(ToTrinhThamDinhNhaThauThemMoiDto Dto)
    : IRequest<ToTrinhThamDinhNhaThauThemMoiResult>;

internal class ToTrinhThamDinhNhaThauThemMoiCommandHandler
    : IRequestHandler<ToTrinhThamDinhNhaThauThemMoiCommand, ToTrinhThamDinhNhaThauThemMoiResult> {
    private readonly IRepository<ToTrinhThamDinhNhaThau, Guid> _repo;
    private readonly IRepository<ToTrinhThamDinhBuocXuLy, long> _buocXuLyRepo;
    private readonly IRepository<ToTrinhQuyetDinh, long> _toTrinhQuyetDinhRepo;
    private readonly IRepository<VanBanQuyetDinh, Guid> _vanBanQuyetDinhRepo;
    private readonly IRepository<GoiThau, Guid> _goiThauRepo;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepo;
    private readonly IAuthorizationManager _authManager;
    private readonly IAuthorizationContext _authContext;
    private readonly IUnitOfWork _unitOfWork;

    public ToTrinhThamDinhNhaThauThemMoiCommandHandler(IServiceProvider serviceProvider) {
        _repo = serviceProvider.GetRequiredService<IRepository<ToTrinhThamDinhNhaThau, Guid>>();
        _buocXuLyRepo = serviceProvider.GetRequiredService<IRepository<ToTrinhThamDinhBuocXuLy, long>>();
        _toTrinhQuyetDinhRepo = serviceProvider.GetRequiredService<IRepository<ToTrinhQuyetDinh, long>>();
        _vanBanQuyetDinhRepo = serviceProvider.GetRequiredService<IRepository<VanBanQuyetDinh, Guid>>();
        _goiThauRepo = serviceProvider.GetRequiredService<IRepository<GoiThau, Guid>>();
        _statusRepo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _authManager = serviceProvider.GetRequiredService<IAuthorizationManager>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _unitOfWork = _repo.UnitOfWork;
    }

    public async Task<ToTrinhThamDinhNhaThauThemMoiResult> Handle(ToTrinhThamDinhNhaThauThemMoiCommand request,
        CancellationToken cancellationToken = default) {
        var dto = request.Dto;

        await _authManager.EnsureCanExecuteAsync(dto.BuocId, dto.DuAnId, _authContext, cancellationToken);

        var goiThauTonTai = await _goiThauRepo.GetQueryableSet()
            .AnyAsync(e => e.Id == dto.GoiThauId, cancellationToken);
        ManagedException.ThrowIf(!goiThauTonTai, "Không tìm thấy gói thầu");

        var trangThaiDuThao = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DeXuatMacDinh.DuThao && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);

        var entity = new ToTrinhThamDinhNhaThau {
            Id = GuidExtensions.GetSequentialGuidId(),
            DuAnId = dto.DuAnId,
            BuocId = dto.BuocId,
            GoiThauId = dto.GoiThauId,
            So = dto.So ?? string.Empty,
            NgayTrinh = dto.NgayTrinh ?? DateTimeOffset.UtcNow,
            TrichYeu = dto.TrichYeu,
            TrangThaiDangTaiId = dto.TrangThaiDangTaiId,
            TrangThaiId = trangThaiDuThao?.Id,
            TenNhaThau = dto.ThongTinNhaThau?.TenNhaThau,
            NgayKetThucDanhGia = dto.ThongTinNhaThau?.NgayKetThucDanhGia,
        };

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await _repo.AddAsync(entity, cancellationToken);

        foreach (var (buocDto, loai) in new (ThongTinBuocXuLyDto? Dto, ELoaiBuocXuLyThamDinhNhaThau Loai)[] {
            (dto.ThongTinDoiChieu, ELoaiBuocXuLyThamDinhNhaThau.DoiChieu),
            (dto.ThongTinThuongThao, ELoaiBuocXuLyThamDinhNhaThau.ThuongThao),
            (dto.ThongTinThamDinh, ELoaiBuocXuLyThamDinhNhaThau.ThamDinh),
        }) {
            if (buocDto == null) continue;
            await _buocXuLyRepo.AddAsync(new ToTrinhThamDinhBuocXuLy {
                ToTrinhId = entity.Id,
                So = buocDto.So,
                Ngay = buocDto.Ngay,
                NoiDung = buocDto.NoiDung,
                Loai = (int)loai,
            }, cancellationToken);
        }

        ToTrinhQuyetDinh? toTrinhQuyetDinh = null;
        if (dto.ToTrinhKetQua != null) {
            toTrinhQuyetDinh = new ToTrinhQuyetDinh {
                EntityId = entity.Id,
                Loai = (int)ELoaiToTrinhQuyetDinh.ToTrinhThamDinhNhaThau,
                So = dto.ToTrinhKetQua.So,
                Ngay = dto.ToTrinhKetQua.Ngay,
                NguoiKy = dto.ToTrinhKetQua.NguoiKy,
                ChucVu = dto.ToTrinhKetQua.ChucVuId,
                TrichYeu = dto.ToTrinhKetQua.TrichYeu,
            };
            await _toTrinhQuyetDinhRepo.AddAsync(toTrinhQuyetDinh, cancellationToken);
        }

        VanBanQuyetDinh? vanBanQuyetDinh = null;
        if (dto.QuyetDinhPheDuyet != null) {
            // Quyết định mới của Tờ trình thẩm định nhà thầu PHẢI ở trạng thái CHỜ DUYỆT,
            // không được để TrangThaiDuyetId = null (khác dữ liệu cũ) — mục 20-21 yêu cầu task #179.
            var trangThaiChoDuyet = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
                .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.ToTrinhThamDinhNhaThauQuyetDinh.ChoDuyet
                    && s.Loai == PheDuyetEntityNames.ToTrinhThamDinhNhaThau, cancellationToken);
            ManagedException.ThrowIfNull(trangThaiChoDuyet, "Không tìm thấy trạng thái 'Chờ duyệt' cho Quyết định phê duyệt");

            vanBanQuyetDinh = new VanBanQuyetDinh {
                DuAnId = entity.DuAnId,
                BuocId = entity.BuocId,
                So = dto.QuyetDinhPheDuyet.So,
                Ngay = dto.QuyetDinhPheDuyet.Ngay,
                NguoiKy = dto.QuyetDinhPheDuyet.NguoiKy,
                NgayKy = dto.QuyetDinhPheDuyet.NgayKy,
                NguoiKyChucVuId = dto.QuyetDinhPheDuyet.ChucVuId,
                TrichYeu = dto.QuyetDinhPheDuyet.TrichYeu,
                Loai = nameof(EnumLoaiVanBanQuyetDinh.ToTrinhThamDinhNhaThau),
                TrangThaiDuyetId = trangThaiChoDuyet!.Id,
            };
            await _vanBanQuyetDinhRepo.AddAsync(vanBanQuyetDinh, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return new ToTrinhThamDinhNhaThauThemMoiResult(entity, toTrinhQuyetDinh?.Id, vanBanQuyetDinh?.Id);
    }
}
