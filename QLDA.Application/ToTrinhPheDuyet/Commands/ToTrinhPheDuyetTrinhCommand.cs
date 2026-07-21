using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common;
using QLDA.Domain.Constants;
using System.Reflection;
using Serilog;

namespace QLDA.Application.ToTrinhPheDuyets.Commands;

/// <summary>
/// Trình quyết định điều chỉnh - chỉ phòng KH-TC (PhongBanId = 219)
/// </summary>
public record ToTrinhPheDuyetTrinhCommand(Guid Id, string Loai, string? NoiDung = null) : IRequest<int>;

internal class ToTrinhPheDuyetTrinhCommandHandler : IRequestHandler<ToTrinhPheDuyetTrinhCommand, int> {
    private readonly DbContext _dbContext;
    private readonly IRepository<ToTrinhPheDuyet, Guid> _repository;
   // private readonly IRepository<QuyetDinhDuyetDuToan, Guid> _quyetDinhDuyetDuToan;
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


        string table = request.Loai;//QuyetDinhDuyetDuToan
        if (ToTrinhEntityNamesExtensions.ContainsEntity(request.Loai))
            table = "ToTrinhPheDuyet";

        var entityType = _dbContext.Model.GetEntityTypes()
                .FirstOrDefault(t => t.ClrType.Name == table)?.ClrType;

        ManagedException.ThrowIfNull(entityType, "Không tìm thấy quyết định/tờ trình cần thao tác");
        try {


            object[] keyValues = { request.Id };

            var entity = await _dbContext.FindAsync(entityType, keyValues,cancellationToken);

            if (entity is IApprovableEntity approvableEntity) {

                var entitySafe = approvableEntity!;
                await _auth.EnsureCanExecuteStepAsync(entitySafe.BuocId, _authContext, cancellationToken);

                // Validate: must be DT (Dự thảo) or TL (Trả lại) to transition to ĐTr (Đã trình)
                if (entitySafe.TrangThaiId != trangThaiDuThao?.Id && entitySafe.TrangThaiId != trangThaiTraLai?.Id) {
                    throw new ManagedException("Chỉ có thể trình khi trạng thái là dự thảo hoặc trả lại!");
                }

                entitySafe.TrangThaiId = trangThaiDaTrinh!.Id;

                // 6. Lưu lịch sử phê duyệt
                var history = new PheDuyetHistory {
                    Id = Guid.NewGuid(),
                    EntityName = request.Loai,
                    EntityId = request.Id,
                    DuAnId = entitySafe.DuAnId,
                    BuocId = entitySafe.BuocId,
                    NguoiXuLyId = _userProvider.Info.UserID,
                    TrangThaiId = trangThaiDaTrinh!.Id,
                    NoiDung = request.NoiDung,


                    NoiDung = $"Số {entitySafe.So ?? ""} " +
                      entity.NgayTrinh != null ? $"- ngày  {entity.NgayTrinh.ToDateOnlyVn()?.ToString("dd/MM/yyyy")}" : "" +
                      $"{(!string.IsNullOrEmpty(request.NoiDung) ? " với nội dung: " + request.NoiDung : " ")}",
                    NgayXuLy = DateTimeOffset.UtcNow
                };
                await _historyRepository.AddAsync(history);

                // 7. Lưu thay đổi vào DB thông qua DbContext
                await _dbContext.SaveChangesAsync(cancellationToken);

                return 1;

            } else {
                throw new ManagedException("Không tìm thấy dữ liệu cần thao tác trong hệ thống!");
            }
        } catch (Exception ex) {
            Log.Information("erro :" + ex.Message);
            throw;
        }
    }


}
