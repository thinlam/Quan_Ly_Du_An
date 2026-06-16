using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QLDA.Application.QuyetDinhDuyetDuToans.DTOs;
using QLDA.Domain.Constants;
using System.Data;

namespace QLDA.Application.QuyetDinhDuyetDuToans.Commands;

public record QuyetDinhDuyetDuToanInsertCommand(QuyetDinhDuyetDuToan Dto) : IRequest<QuyetDinhDuyetDuToan>;

internal class QuyetDinhDuyetDuToanInsertCommandHandler : IRequestHandler<QuyetDinhDuyetDuToanInsertCommand, QuyetDinhDuyetDuToan> {
    private readonly IRepository<QuyetDinhDuyetDuToan, Guid> QuyetDinhDuyetDuToan;
    private readonly IRepository<QuyetDinhDuyetDuToanNguonVon, Guid> KeHoachVons;
    private readonly IRepository<QuyetDinhDuyetDuToanChiPhi, Guid> KeHoachChiPhis;
    private readonly IRepository<DuAn, Guid> DuAn;
    private readonly IRepository<DanhMucBuoc, int> DanhMucBuoc;
    private readonly IUnitOfWork UnitOfWork;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> StatusRepo;
    private readonly ILogger<QuyetDinhDuyetDuToanInsertCommandHandler> Logger;

    public QuyetDinhDuyetDuToanInsertCommandHandler(IServiceProvider serviceProvider,
        ILogger<QuyetDinhDuyetDuToanInsertCommandHandler> logger) {
        QuyetDinhDuyetDuToan = serviceProvider.GetRequiredService<IRepository<QuyetDinhDuyetDuToan, Guid>>();
        KeHoachVons = serviceProvider.GetRequiredService<IRepository<QuyetDinhDuyetDuToanNguonVon, Guid>>();
        KeHoachChiPhis = serviceProvider.GetRequiredService<IRepository<QuyetDinhDuyetDuToanChiPhi, Guid>>();
        DuAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
        DanhMucBuoc = serviceProvider.GetRequiredService<IRepository<DanhMucBuoc, int>>();
        StatusRepo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        Logger = logger;
        UnitOfWork = QuyetDinhDuyetDuToan.UnitOfWork;
    }

    public async Task<QuyetDinhDuyetDuToan> Handle(QuyetDinhDuyetDuToanInsertCommand request, CancellationToken cancellationToken = default) {
        try {
            ManagedException.ThrowIf(!DuAn.GetQueryableSet().Any(e => e.Id == request.Dto.DuAnId),
                "Không tồn tại dự án");

            var entity = request.Dto;
            var trangThaiDuThao = await StatusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
                                .FirstOrDefaultAsync(s => s.Ma == "DT" && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);

            entity.TrangThaiId = trangThaiDuThao.Id;

            using (await UnitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken)) {
                await QuyetDinhDuyetDuToan.AddAsync(entity, cancellationToken);
                await UnitOfWork.SaveChangesAsync(cancellationToken);

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