using System.Reflection;
using BuildingBlocks.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common;
using QLDA.Domain.Constants;
using Serilog;

namespace QLDA.Application.ToTrinhPheDuyets.Commands;

/// <summary>
/// Trình quyết định điều chỉnh - chỉ phòng KH-TC (PhongBanId = 219)
/// </summary>
public record ToTrinhPheDuyetTrinhCommand(Guid Id, string Loai, string? NoiDung = null) : IRequest<int>;

internal class ToTrinhPheDuyetTrinhCommandHandler : IRequestHandler<ToTrinhPheDuyetTrinhCommand, int> {
    private readonly DbContext _dbContext;
    private readonly IRepository<ToTrinhPheDuyet, Guid> _repository;
    private readonly IRepository<PheDuyetHistory, Guid> _historyRepository;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationContext _authContext;
    private readonly IUserProvider _userProvider;
    private readonly IUnitOfWork _unitOfWork;

    public ToTrinhPheDuyetTrinhCommandHandler(DbContext dbContext, IServiceProvider serviceProvider) {
        _dbContext = dbContext;
        _repository = serviceProvider.GetRequiredService<IRepository<ToTrinhPheDuyet, Guid>>();
        _historyRepository = serviceProvider.GetRequiredService<IRepository<PheDuyetHistory, Guid>>();
        _statusRepository = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task<int> Handle(ToTrinhPheDuyetTrinhCommand request, CancellationToken cancellationToken) {
        var statuses = await _statusRepository.GetByLoaiAsync(PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);
        var statusDict = statuses
            .Where(x => !string.IsNullOrWhiteSpace(x.Ma))
            .ToDictionary(x => x.Ma!, x => x);

        var trangThaiDuThao = statusDict.GetValueOrDefault(TrangThaiPheDuyetCodes.DeXuatMacDinh.DuThao);
        var trangThaiTraLai = statusDict.GetValueOrDefault(TrangThaiPheDuyetCodes.DeXuatMacDinh.TraLai);
        var trangThaiDaTrinh = statusDict.GetValueOrDefault(TrangThaiPheDuyetCodes.DeXuatMacDinh.DaTrinh);

        ManagedException.ThrowIfNull(trangThaiDaTrinh, "Không tìm thấy trạng thái 'Đã trình'");

        var entity = await _repository.GetQueryableSet().AsNoTracking()
       .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
        //if (ToTrinhEntityNamesExtensions.ContainsEntity(request.Loai))
        //    table = "ToTrinhPheDuyet";

        //var entityType = _dbContext.Model.GetEntityTypes()
        //        .FirstOrDefault(t => t.ClrType.Name == table)?.ClrType;

        ManagedException.ThrowIfNull(entity, "Không tìm thấy quyết định/tờ trình cần thao tác");

        //object[] keyValues = { request.Id };

        //if (entity is IApprovableEntity approvableEntity) {

        //var entitySafe = approvableEntity!;
        //await _auth.EnsureCanExecuteStepAsync(entitySafe.BuocId, _authContext, cancellationToken);

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
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        return 1;


    }

}

