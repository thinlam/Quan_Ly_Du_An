using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace QLDA.Application.QuyetDinhDuyetKHLCNTs.Commands;

public record QuyetDinhDuyetKHLCNTInsertOrUpdateCommand(QuyetDinhDuyetKHLCNT Entity) : IRequest {
}

internal class
    QuyetDinhDuyetKHLCNTInsertOrUpdateCommandHandler : IRequestHandler<QuyetDinhDuyetKHLCNTInsertOrUpdateCommand> {
    private readonly IRepository<KeHoachLuaChonNhaThau, Guid> KeHoachLuaChonNhaThau;
    private readonly IRepository<QuyetDinhDuyetKHLCNT, Guid> QuyetDinhDuyetKHLCNT;
    private readonly IRepository<DuAn, Guid> DuAn;
    private readonly IRepository<DanhMucBuoc, int> DanhMucBuoc;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<QuyetDinhDuyetKHLCNTInsertOrUpdateCommandHandler> _logger;

    public QuyetDinhDuyetKHLCNTInsertOrUpdateCommandHandler(IServiceProvider serviceProvider,
        ILogger<QuyetDinhDuyetKHLCNTInsertOrUpdateCommandHandler> logger) {
        QuyetDinhDuyetKHLCNT = serviceProvider.GetRequiredService<IRepository<QuyetDinhDuyetKHLCNT, Guid>>();
        KeHoachLuaChonNhaThau = serviceProvider.GetRequiredService<IRepository<KeHoachLuaChonNhaThau, Guid>>();
        DuAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
        DanhMucBuoc = serviceProvider.GetRequiredService<IRepository<DanhMucBuoc, int>>();
        _logger = logger;
        _unitOfWork = QuyetDinhDuyetKHLCNT.UnitOfWork;
    }

    public async Task Handle(QuyetDinhDuyetKHLCNTInsertOrUpdateCommand request,
        CancellationToken cancellationToken = default) {
        try {
            ManagedException.ThrowIf(!DuAn.GetQueryableSet().Any(e => e.Id == request.Entity.VanBanQuyetDinh.DuAnId),
                "Không tồn tại dự án");

            using (await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken)) {
                var isExist = QuyetDinhDuyetKHLCNT.GetQueryableSet().Any(o => o.Id == request.Entity.Id);
                if (isExist) {
                    await QuyetDinhDuyetKHLCNT.UpdateAsync(request.Entity, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                } else {
                    //Thêm dự án trước
                    await QuyetDinhDuyetKHLCNT.AddAsync(request.Entity, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
                await HandleKeHoachLuaChonNhaThau(request,cancellationToken);
                //Cập nhật quy trình
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }
        } catch (Exception ex) {
            _logger.LogError(ex, ex.Message);
            throw;
        }
    }

    private async Task HandleKeHoachLuaChonNhaThau(QuyetDinhDuyetKHLCNTInsertOrUpdateCommand request,
        CancellationToken cancellationToken = default) {
        var keHoachLuaChonNhaThau = await KeHoachLuaChonNhaThau.GetQueryableSet()
            .Include(e => e.GoiThaus)
            .FirstOrDefaultAsync(e => e.Id == request.Entity.KeHoachLuaChonNhaThauId, cancellationToken);
        ManagedException.ThrowIfNull(keHoachLuaChonNhaThau);
        if (keHoachLuaChonNhaThau.GoiThaus == null || keHoachLuaChonNhaThau.GoiThaus.Count == 0) return;
        // foreach (var item in keHoachLuaChonNhaThau.GoiThaus) {
        //     item.DaDuyet = true;
        // }
    }
}