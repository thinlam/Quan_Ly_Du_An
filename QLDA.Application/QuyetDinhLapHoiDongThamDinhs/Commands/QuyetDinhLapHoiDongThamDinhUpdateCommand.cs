using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QLDA.Application.QuyetDinhLapHoiDongThamDinhs.DTOs;

namespace QLDA.Application.QuyetDinhLapHoiDongThamDinhs.Commands;

public record QuyetDinhLapHoiDongThamDinhUpdateCommand(QuyetDinhLapHoiDongThamDinhUpdateDto Dto) : IRequest<QuyetDinhLapHoiDongThamDinh>;

internal class QuyetDinhLapHoiDongThamDinhUpdateCommandHandler : IRequestHandler<QuyetDinhLapHoiDongThamDinhUpdateCommand, QuyetDinhLapHoiDongThamDinh> {
    private readonly IRepository<QuyetDinhLapHoiDongThamDinh, Guid> QuyetDinhLapHoiDongThamDinh;
    private readonly IRepository<DuAn, Guid> DuAn;
    private readonly IRepository<DanhMucBuoc, int> DanhMucBuoc;
    private readonly IUnitOfWork UnitOfWork;
    private readonly ILogger<QuyetDinhLapHoiDongThamDinhUpdateCommandHandler> Logger;

    public QuyetDinhLapHoiDongThamDinhUpdateCommandHandler(IServiceProvider serviceProvider,
        ILogger<QuyetDinhLapHoiDongThamDinhUpdateCommandHandler> logger) {
        QuyetDinhLapHoiDongThamDinh = serviceProvider.GetRequiredService<IRepository<QuyetDinhLapHoiDongThamDinh, Guid>>();
        DuAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
        DanhMucBuoc = serviceProvider.GetRequiredService<IRepository<DanhMucBuoc, int>>();
        Logger = logger;
        UnitOfWork = QuyetDinhLapHoiDongThamDinh.UnitOfWork;
    }

    public async Task<QuyetDinhLapHoiDongThamDinh> Handle(QuyetDinhLapHoiDongThamDinhUpdateCommand request, CancellationToken cancellationToken = default) {
        try {
           
            var entity = await QuyetDinhLapHoiDongThamDinh.GetQueryableSet()
                                    .FirstOrDefaultAsync(e => e.Id == request.Dto.Id, cancellationToken);
            ManagedException.ThrowIfNull(entity, "Không tìm thấy dữ liệu.");
            request.Dto.ToEntity(entity);

            using (await UnitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken)) {
                await QuyetDinhLapHoiDongThamDinh.UpdateAsync(entity, cancellationToken);
                await UnitOfWork.SaveChangesAsync(cancellationToken);
                await UnitOfWork.CommitTransactionAsync(cancellationToken);
            }

            return entity;
        } catch (Exception ex) {
            Logger.LogError(ex, ex.Message);
            throw;
        }
    }
}
