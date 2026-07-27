using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common;
using QLDA.Domain.Constants;
using System.Reflection;
using Serilog;
using System.Data;

namespace QLDA.Application.QuyetDinhDuyetDuToans.Commands;

/// <summary>
/// Trình quyết định duyệt dự toán
/// Trình tách riêng từ toTrinhPheDuyetTrinhCommand -> trình get tờ trình/ngày/trích yếu lưu xuống PheDuyet
/// Các thao tác trả/duyệt vẫn dùng chung với ToTrinhPheDuyetxxxCommand 
/// </summary>
public record QuyetDinhDuyetDuToanTrinhCommand(Guid Id, string Loai, string? NoiDung = null) : IRequest<int>;

internal class QuyetDinhDuyetDuToanTrinhCommandHandler : IRequestHandler<QuyetDinhDuyetDuToanTrinhCommand, int> {
    private readonly DbContext _dbContext;
    private readonly IRepository<QuyetDinhDuyetDuToan, Guid> _quyetDinhDuyetDuToan;
    private readonly IRepository<PheDuyetHistory, Guid> _historyRepository;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationContext _authContext;
    private readonly IUserProvider _userProvider;
    private readonly IUnitOfWork _unitOfWork;

    public QuyetDinhDuyetDuToanTrinhCommandHandler(DbContext dbContext, IServiceProvider serviceProvider) {
        _dbContext = dbContext;
        _quyetDinhDuyetDuToan = serviceProvider.GetRequiredService<IRepository<QuyetDinhDuyetDuToan, Guid>>();
        _historyRepository = serviceProvider.GetRequiredService<IRepository<PheDuyetHistory, Guid>>();
        _statusRepository = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _unitOfWork = _quyetDinhDuyetDuToan.UnitOfWork;
    }

    public async Task<int> Handle(QuyetDinhDuyetDuToanTrinhCommand request, CancellationToken cancellationToken) {
        var statuses = await _statusRepository.GetByLoaiAsync(PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);
        var statusDict = statuses
            .Where(x => !string.IsNullOrWhiteSpace(x.Ma))
            .ToDictionary(x => x.Ma!, x => x);

        var trangThaiDuThao = statusDict.GetValueOrDefault(TrangThaiPheDuyetCodes.DeXuatMacDinh.DuThao);
        var trangThaiTraLai = statusDict.GetValueOrDefault(TrangThaiPheDuyetCodes.DeXuatMacDinh.TraLai);
        var trangThaiDaTrinh = statusDict.GetValueOrDefault(TrangThaiPheDuyetCodes.DeXuatMacDinh.DaTrinh);

        ManagedException.ThrowIfNull(trangThaiDaTrinh, "Không tìm thấy trạng thái 'Đã trình'");

        var entity = await _quyetDinhDuyetDuToan.GetQueryableSet().AsNoTracking()
         .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity, "Không tìm thấy quyết định/tờ trình cần thao tác");
        try {

            await _auth.EnsureCanExecuteStepAsync(entity.BuocId, _authContext, cancellationToken);

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
                NoiDung =   $"Số {entity.So} {(entity.Ngay != null ? " - ngày " + entity.Ngay.ToDateOnlyVn()?.ToString("dd/MM/yyyy") : "")}" +
                            $"{(!string.IsNullOrEmpty( entity.TrichYeu ) ? " - " + entity.TrichYeu : "")} " +
                            $"{(!string.IsNullOrEmpty(request.NoiDung) ? " với nội dung: " + request.NoiDung : " ")}",
                NgayXuLy = DateTimeOffset.UtcNow
            };
            using (await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken)) {
                await _historyRepository.AddAsync(history);
                await _quyetDinhDuyetDuToan.UpdateAsync(entity, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }

            // 7. Lưu thay đổi vào DB thông qua DbContext

            return 1;


        } catch (Exception ex) {
            Log.Information("error :" + ex.Message);
            throw;
        }
    }


}
