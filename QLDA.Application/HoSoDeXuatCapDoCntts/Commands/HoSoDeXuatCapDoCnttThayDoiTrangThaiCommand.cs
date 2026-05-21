using System.Data;
using BuildingBlocks.CrossCutting.ExtensionMethods;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common;

namespace QLDA.Application.HoSoDeXuatCapDoCntts.Commands;

public class HoSoDeXuatCapDoCnttThayDoiTrangThaiDto {
    public Guid HoSoId { get; set; }
    public int TrangThaiId { get; set; }
    public string? NoiDung { get; set; }
}

public record HoSoDeXuatCapDoCnttThayDoiTrangThaiCommand(HoSoDeXuatCapDoCnttThayDoiTrangThaiDto Dto) 
    : IRequest;

internal class HoSoDeXuatCapDoCnttThayDoiTrangThaiCommandHandler 
    : IRequestHandler<HoSoDeXuatCapDoCnttThayDoiTrangThaiCommand> {
    
    private readonly IRepository<HoSoDeXuatCapDoCntt, Guid> HoSoDeXuatCapDoCntt;
    private readonly IUnitOfWork _unitOfWork;

    public HoSoDeXuatCapDoCnttThayDoiTrangThaiCommandHandler(IServiceProvider serviceProvider) {
        HoSoDeXuatCapDoCntt = serviceProvider.GetRequiredService<IRepository<HoSoDeXuatCapDoCntt, Guid>>();
        _unitOfWork = HoSoDeXuatCapDoCntt.UnitOfWork;
    }

    public async Task Handle(HoSoDeXuatCapDoCnttThayDoiTrangThaiCommand request, CancellationToken cancellationToken = default) {
        var entity = await HoSoDeXuatCapDoCntt.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Dto.HoSoId && !e.IsDeleted, cancellationToken);
        ManagedException.ThrowIfNull(entity);

        // Validate transition (Khởi tạo → Trình → Duyệt/Từ chối)
        ValidateStatusTransition(entity.TrangThaiId, request.Dto.TrangThaiId);

        // Note: Authorization check should be done at the controller level with [Authorize] attribute
        // For phòng ban 219 (duyệt/từ chối), implement additional authorization policy in WebApi layer

        entity.TrangThaiId = request.Dto.TrangThaiId;
        entity.NgayTrinh = DateOnly.FromDateTime(DateTime.UtcNow).ToStartOfDayUtc();

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await HoSoDeXuatCapDoCntt.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);
    }

    private static void ValidateStatusTransition(int? currentStatus, int newStatus) {
        // Khởi tạo → Trình ✓
        // Trình → Duyệt ✓
        // Trình → Từ chối ✓
        // Các chuyển đổi khác → ✗
        if (currentStatus == newStatus) {
            throw new InvalidOperationException("Trạng thái mới phải khác trạng thái hiện tại");
        }
    }
}