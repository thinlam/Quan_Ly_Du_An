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
    private readonly IRepository<ToTrinhQuyetDinh, long> _toTrinhQuyetDinhRepo;
    private readonly IRepository<VanBanQuyetDinh, Guid> _vanBanQuyetDinhRepo;
    private readonly IRepository<GoiThau, Guid> _goiThauRepo;
    private readonly IRepository<DanhMucNhaThau, Guid> _nhaThauRepo;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepo;
    private readonly IAuthorizationManager _authManager;
    private readonly IAuthorizationContext _authContext;
    private readonly IUnitOfWork _unitOfWork;

    public ToTrinhThamDinhNhaThauThemMoiCommandHandler(IServiceProvider serviceProvider) {
        _repo = serviceProvider.GetRequiredService<IRepository<ToTrinhThamDinhNhaThau, Guid>>();
        _toTrinhQuyetDinhRepo = serviceProvider.GetRequiredService<IRepository<ToTrinhQuyetDinh, long>>();
        _vanBanQuyetDinhRepo = serviceProvider.GetRequiredService<IRepository<VanBanQuyetDinh, Guid>>();
        _goiThauRepo = serviceProvider.GetRequiredService<IRepository<GoiThau, Guid>>();
        _nhaThauRepo = serviceProvider.GetRequiredService<IRepository<DanhMucNhaThau, Guid>>();
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

        if (dto.ThongTinNhaThau?.NhaThauId is { } nhaThauId && nhaThauId != Guid.Empty) {
            var nhaThauTonTai = await _nhaThauRepo.GetQueryableSet()
                .AnyAsync(e => e.Id == nhaThauId, cancellationToken);
            ManagedException.ThrowIf(!nhaThauTonTai, "Không tìm thấy nhà thầu");
        }

        // Dùng lại đúng convention 4 trạng thái chung (DT/ĐTr/ĐD/TL) của DeXuatMacDinh —
        // không tạo bộ trạng thái riêng cho Tờ trình thẩm định nhà thầu.
        var trangThaiDuThao = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DeXuatMacDinh.DuThao && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);

        var entity = new ToTrinhThamDinhNhaThau {
            Id = GuidExtensions.GetSequentialGuidId(),
            DuAnId = dto.DuAnId,
            BuocId = dto.BuocId,
            GoiThauId = dto.GoiThauId,
            TrangThaiDangTaiId = dto.TrangThaiDangTaiId,
            TrangThaiId = trangThaiDuThao?.Id,
            NhaThauId = dto.ThongTinNhaThau?.NhaThauId is { } id && id != Guid.Empty ? id : null,
            NgayKetThucDanhGia = dto.ThongTinNhaThau?.NgayKetThucDanhGia,
        };
        entity.SyncBuocXuLys(ToTrinhThamDinhNhaThauMappings.ToBuocXuLyList(dto.DoiChieu, dto.ThuongThao, dto.ThamDinh));

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await _repo.AddAsync(entity, cancellationToken);

        ToTrinhQuyetDinh? toTrinhQuyetDinh = null;
        if (dto.ToTrinhKetQua != null) {
            toTrinhQuyetDinh = new ToTrinhQuyetDinh {
                EntityId = entity.Id,
                Loai = ToTrinhQuyetDinhLoai.ToTrinhThamDinhNhaThau,
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
            // Id = entity.Id (giống pattern HoSoMoiThauDienTuDuyetCommand) để ToTrinhThamDinhNhaThauDuyetCommand
            // (dispatch qua QuanLyPheDuyet) tra được đúng VanBanQuyetDinh cần đồng bộ trạng thái khi duyệt.
            // TrangThaiDuyetId đồng bộ với TrangThaiId của Tờ trình (Dự thảo) — không tạo trạng thái "Chờ duyệt" riêng.
            vanBanQuyetDinh = new VanBanQuyetDinh {
                Id = entity.Id,
                DuAnId = entity.DuAnId,
                BuocId = entity.BuocId,
                So = dto.QuyetDinhPheDuyet.So,
                Ngay = dto.QuyetDinhPheDuyet.Ngay,
                NguoiKy = dto.QuyetDinhPheDuyet.NguoiKy,
                NgayKy = dto.QuyetDinhPheDuyet.NgayKy,
                NguoiKyChucVuId = dto.QuyetDinhPheDuyet.ChucVuId,
                TrichYeu = dto.QuyetDinhPheDuyet.TrichYeu,
                Loai = nameof(EnumLoaiVanBanQuyetDinh.ToTrinhThamDinhNhaThau),
                TrangThaiDuyetId = trangThaiDuThao?.Id,
            };
            await _vanBanQuyetDinhRepo.AddAsync(vanBanQuyetDinh, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return new ToTrinhThamDinhNhaThauThemMoiResult(entity, toTrinhQuyetDinh?.Id, vanBanQuyetDinh?.Id);
    }
}
