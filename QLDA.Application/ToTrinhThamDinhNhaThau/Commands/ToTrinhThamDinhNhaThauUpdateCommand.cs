using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.ToTrinhThamDinhNhaThaus.DTOs;
using QLDA.Domain.Constants;
using QLDA.Domain.Enums;

namespace QLDA.Application.ToTrinhThamDinhNhaThaus.Commands;

/// <summary>
/// Kết quả cập nhật — trả thêm Id của ToTrinhQuyetDinh/VanBanQuyetDinh để Controller
/// lưu TepDinhKem đúng GroupId cho từng mục (Issue #179).
/// </summary>
public record ToTrinhThamDinhNhaThauUpdateResult(
    ToTrinhThamDinhNhaThau Entity,
    long? ToTrinhQuyetDinhId,
    Guid? VanBanQuyetDinhId);

public record ToTrinhThamDinhNhaThauUpdateCommand(ToTrinhThamDinhNhaThauCapNhatDto Dto)
    : IRequest<ToTrinhThamDinhNhaThauUpdateResult>;

internal class ToTrinhThamDinhNhaThauUpdateCommandHandler : IRequestHandler<ToTrinhThamDinhNhaThauUpdateCommand, ToTrinhThamDinhNhaThauUpdateResult> {
    private readonly IRepository<ToTrinhThamDinhNhaThau, Guid> _repo;
    private readonly IRepository<ToTrinhQuyetDinh, long> _toTrinhQuyetDinhRepo;
    private readonly IRepository<VanBanQuyetDinh, Guid> _vanBanQuyetDinhRepo;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepo;
    private readonly IAuthorizationManager _authManager;
    private readonly IAuthorizationContext _authContext;
    private readonly IUnitOfWork _unitOfWork;

    public ToTrinhThamDinhNhaThauUpdateCommandHandler(IServiceProvider serviceProvider) {
        _repo = serviceProvider.GetRequiredService<IRepository<ToTrinhThamDinhNhaThau, Guid>>();
        _toTrinhQuyetDinhRepo = serviceProvider.GetRequiredService<IRepository<ToTrinhQuyetDinh, long>>();
        _vanBanQuyetDinhRepo = serviceProvider.GetRequiredService<IRepository<VanBanQuyetDinh, Guid>>();
        _statusRepo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _authManager = serviceProvider.GetRequiredService<IAuthorizationManager>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _unitOfWork = _repo.UnitOfWork;
    }

    public async Task<ToTrinhThamDinhNhaThauUpdateResult> Handle(ToTrinhThamDinhNhaThauUpdateCommand request,
        CancellationToken cancellationToken = default) {
        var dto = request.Dto;
        var trangThaiDuThao = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DeXuatMacDinh.DuThao && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);
        var trangThaiTraLai = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
        .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DeXuatMacDinh.TraLai && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);

        var entity = await _repo.GetQueryableSet()
            .Include(e => e.BuocXuLys)
            .Include(e => e.TrangThai)
            .FirstOrDefaultAsync(e => e.Id == dto.Id, cancellationToken);
        ManagedException.ThrowIf(entity == null, "Không tìm thấy dữ liệu.");

        await _authManager.EnsureCanExecuteAsync(entity.BuocId, entity.DuAnId, _authContext, cancellationToken);

        if (entity.TrangThaiId != trangThaiDuThao?.Id && entity.TrangThaiId != trangThaiTraLai?.Id)
        {
            throw new ManagedException("Trạng thái không thể cập nhật!");
        }
        entity.DuAnId = dto.DuAnId;
        entity.BuocId = dto.BuocId;
        if (dto.GoiThauId.HasValue)
            entity.GoiThauId = dto.GoiThauId;
        entity.TrangThaiDangTaiId = dto.TrangThaiDangTaiId;
        // Thông tin nhà thầu — ưu tiên object ThongTinNhaThau, fallback NhaThauId top-level (Issue #179).
        if (dto.ThongTinNhaThau != null || dto.NhaThauId.HasValue)
            entity.NhaThauId = dto.ThongTinNhaThau?.NhaThauId ?? dto.NhaThauId;
        if (dto.ThongTinNhaThau != null)
            entity.NgayKetThucDanhGia = dto.ThongTinNhaThau.NgayKetThucDanhGia;


        entity.GiaTriTrungThau = dto.GiaTriTrungThau;
        entity.SoNgayThucHienHopDong = dto.SoNgayThucHienHopDong;
        entity.ThoiGianThucHienGoiThau = dto.ThoiGianThucHienGoiThau;   


        entity.SyncBuocXuLys(ToTrinhThamDinhNhaThauMappings.ToBuocXuLyList(dto.DoiChieu, dto.ThuongThao, dto.ThamDinh));

        // Tờ trình kết quả (mục 6) — upsert ToTrinhQuyetDinh theo EntityId + Loai (Issue #179).
        ToTrinhQuyetDinh? toTrinhQuyetDinh = null;
        if (dto.ToTrinhKetQua != null)
        {
            toTrinhQuyetDinh = await _toTrinhQuyetDinhRepo.GetQueryableSet()
                .FirstOrDefaultAsync(x => x.EntityId == entity.Id && x.Loai == ToTrinhQuyetDinhLoai.ToTrinhThamDinhNhaThau, cancellationToken);
            if (toTrinhQuyetDinh != null)
                dto.ToTrinhKetQua.ApplyTo(toTrinhQuyetDinh);
            else
            {
                toTrinhQuyetDinh = dto.ToTrinhKetQua.ToToTrinhQuyetDinh(entity.Id);
                await _toTrinhQuyetDinhRepo.AddAsync(toTrinhQuyetDinh, cancellationToken);
            }
        }

        // Quyết định phê duyệt (mục 7) — upsert VanBanQuyetDinh theo Id + Loai (Issue #179).
        VanBanQuyetDinh? vanBanQuyetDinh = null;
        if (dto.QuyetDinhPheDuyet != null)
        {
            vanBanQuyetDinh = await _vanBanQuyetDinhRepo.GetQueryableSet()
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.Loai == nameof(EnumLoaiVanBanQuyetDinh.ToTrinhThamDinhNhaThau), cancellationToken);
            if (vanBanQuyetDinh != null)
                dto.QuyetDinhPheDuyet.ApplyTo(vanBanQuyetDinh);
            else
            {
                // Trạng thái mới mặc định = Dự thảo, đồng bộ với TrangThaiId của Tờ trình.
                vanBanQuyetDinh = dto.QuyetDinhPheDuyet.ToVanBanQuyetDinh(entity, trangThaiDuThao?.Id);
                await _vanBanQuyetDinhRepo.AddAsync(vanBanQuyetDinh, cancellationToken);
            }
        }

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await _repo.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return new ToTrinhThamDinhNhaThauUpdateResult(entity!, toTrinhQuyetDinh?.Id, vanBanQuyetDinh?.Id);
    }
}
