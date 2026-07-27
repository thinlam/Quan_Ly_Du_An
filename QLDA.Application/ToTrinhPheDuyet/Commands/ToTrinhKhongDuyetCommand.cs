using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common;
using QLDA.Domain.Constants;

namespace QLDA.Application.ToTrinhPheDuyets.Commands;

/// <summary>

/// Chỉ dành cho các tờ trình/quyết định chỉ trình( k cần duyệt)
/// Bước 1. Lưu vào PheDuyetHistory / PheDuyet( chạy trigger của PheDuyetHistory)
/// Bước 2. Cập nhật tình trạng tờ quyết/quyết định
/// Bước 3. nếu là  tờ trình Kế hoạch lcnt thì lưu vào bảng KeHoachLuuChonNhaThau
/// </summary>
public record ToTrinhKhongDuyetCommand(Guid Id, string Loai, string? NoiDung = null) : IRequest<int>;
internal class ToTrinhKhongDuyetCommandHandler : IRequestHandler<ToTrinhKhongDuyetCommand, int> {
    private readonly DbContext _dbContext;
    private readonly IRepository<ToTrinhPheDuyet, Guid> _repository;
    private readonly IRepository<KeHoachLuaChonNhaThau, Guid> _keHoachRepo;
    private readonly IRepository<PheDuyetHistory, Guid> _historyRepository;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationContext _authContext;
    private readonly IUserProvider _userProvider;
    private readonly IUnitOfWork _unitOfWork;

    public ToTrinhKhongDuyetCommandHandler(DbContext dbContext, IServiceProvider serviceProvider) {
        _dbContext = dbContext;
        _repository = serviceProvider.GetRequiredService<IRepository<ToTrinhPheDuyet, Guid>>();
        _keHoachRepo = serviceProvider.GetRequiredService<IRepository<KeHoachLuaChonNhaThau, Guid>>();
        _historyRepository = serviceProvider.GetRequiredService<IRepository<PheDuyetHistory, Guid>>();
        _statusRepository = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task<int> Handle(ToTrinhKhongDuyetCommand request, CancellationToken cancellationToken) {
        // entity này có 2 loại trạng thái là trạng thái đề xuất mặc định và trạng thái tờ trình ko cần duyệt( trình là xong)

         bool isKhongDuyet = LoaiToTrinhKhongDuyetExtensions.ContainsDescription(request.Loai); //allway true
        var loaiPheDuyet = PheDuyetEntityNames.ToTrinhKhongDuyet;
        var statuses = await _statusRepository.GetByLoaiAsync(loaiPheDuyet, cancellationToken);
        var statusDict = statuses
            .Where(x => !string.IsNullOrWhiteSpace(x.Ma))
            .ToDictionary(x => x.Ma!, x => x);

        var trangThaiDuThao = statusDict.GetValueOrDefault(TrangThaiPheDuyetCodes.DeXuatMacDinh.DuThao);
        var trangThaiTraLai = statusDict.GetValueOrDefault(TrangThaiPheDuyetCodes.DeXuatMacDinh.TraLai);
        var trangThaiDaTrinh = statusDict.GetValueOrDefault(TrangThaiPheDuyetCodes.DeXuatMacDinh.DaTrinh);

        ManagedException.ThrowIfNull(trangThaiDaTrinh, "Không tìm thấy trạng thái 'Đã trình'");

        var entity = await _repository.GetQueryableSet().AsNoTracking()
                        .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
        ManagedException.ThrowIfNull(entity, "Không tìm thấy quyết định/tờ trình cần thao tác");

        await _auth.EnsureCanExecuteStepAsync(entity.BuocId, _authContext, cancellationToken);

        // Validate: must be DT (Dự thảo) or TL (Trả lại) to transition to ĐTr (Đã trình)
        if (entity.TrangThaiId != trangThaiDuThao?.Id && entity.TrangThaiId != trangThaiTraLai?.Id) {
            throw new ManagedException("Chỉ có thể trình khi trạng thái là dự thảo hoặc trả lại!");
        }
        entity.TrangThaiId = trangThaiDaTrinh!.Id;

        // 6. Lưu lịch sử phê duyệt
        var history = new PheDuyetHistory {
            Id = Guid.NewGuid(),
            EntityName = request.Loai,
            EntityId = request.Id,
            DuAnId = entity.DuAnId,
            BuocId = entity.BuocId,
            NguoiXuLyId = _userProvider.Info.UserID,
            TrangThaiId = trangThaiDaTrinh!.Id,
            NoiDung = $"Số {entity.So} {(entity.NgayToTrinh != null ? " - ngày " + entity.NgayToTrinh.ToDateOnlyVn()?.ToString("dd/MM/yyyy") : "")}" +
                $"{(!string.IsNullOrEmpty(entity.TrichYeu) ? " - " + entity.TrichYeu : "")} " +
                $"{(!string.IsNullOrEmpty(request.NoiDung) ? " với nội dung: " + request.NoiDung : " ")}",
            NgayXuLy = DateTimeOffset.UtcNow
        };
        using (await _unitOfWork.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted, cancellationToken)) {
            await _historyRepository.AddAsync(history);
            await _repository.UpdateAsync(entity, cancellationToken);

            #region
            // nếu là tờ trình kế hoạch lcnt  -> duyệt thì insert vào table KeHoachLuaChonNhaThau
            if (Enum.IsDefined(typeof(KeHoachLuaChonNhaThauLoai), request.Loai)) {
                var entityKeHoach = await _repository.GetQueryableSet().FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
                ManagedException.ThrowIfNull(entityKeHoach, "Không tìm thấy kế hoạch cần cập nhật");
                var keHoach = new KeHoachLuaChonNhaThau {
                    Id = Guid.NewGuid(),
                    Ten = entityKeHoach.Ten,
                    Loai = request.Loai,
                    DuAnId = entityKeHoach.DuAnId,
                    BuocId = entityKeHoach.BuocId
                };
                await _keHoachRepo.AddAsync(keHoach, cancellationToken);
            }
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        #endregion
        // 7. Lưu thay đổi vào DB thông qua DbContext

        return 1;
    }
}
