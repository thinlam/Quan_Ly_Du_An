using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QLDA.Application.Authorization;

namespace QLDA.Application.QuyetDinhDuyetDuAns.Commands;

public record QuyetDinhDuyetDuAnInsertOrUpdateCommand(QuyetDinhDuyetDuAn Entity) : IRequest {
}

internal class
    QuyetDinhDuyetDuAnInsertOrUpdateCommandHandler : IRequestHandler<QuyetDinhDuyetDuAnInsertOrUpdateCommand> {
    private readonly IRepository<DuAn, Guid> DuAn;
    private readonly IRepository<DanhMucBuoc, int> DanhMucBuoc;
    private readonly IRepository<QuyetDinhDuyetDuAn, Guid> QuyetDinhDuyetDuAn;
    private readonly IRepository<QuyetDinhDuyetDuAnNguonVon, Guid> QuyetDinhDuyetDuAnNguonVon;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<QuyetDinhDuyetDuAnInsertOrUpdateCommandHandler> _logger;
    private readonly IAuthorizationManager _authManager;
    private readonly IAuthorizationContext _authContext;

    public QuyetDinhDuyetDuAnInsertOrUpdateCommandHandler(IServiceProvider serviceProvider,
        ILogger<QuyetDinhDuyetDuAnInsertOrUpdateCommandHandler> logger) {
        DuAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
        DanhMucBuoc = serviceProvider.GetRequiredService<IRepository<DanhMucBuoc, int>>();
        QuyetDinhDuyetDuAn = serviceProvider.GetRequiredService<IRepository<QuyetDinhDuyetDuAn, Guid>>();
        QuyetDinhDuyetDuAnNguonVon =
            serviceProvider.GetRequiredService<IRepository<QuyetDinhDuyetDuAnNguonVon, Guid>>();
        _logger = logger;
        _unitOfWork = QuyetDinhDuyetDuAn.UnitOfWork;
        _authManager = serviceProvider.GetRequiredService<IAuthorizationManager>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
    }

    public async Task Handle(QuyetDinhDuyetDuAnInsertOrUpdateCommand request,
        CancellationToken cancellationToken = default) {
        try {
            await _authManager.EnsureCanExecuteAsync(request.Entity.BuocId, request.Entity.DuAnId, _authContext, cancellationToken);
            ManagedException.ThrowIf(!DuAn.GetQueryableSet().Any(e => e.Id == request.Entity.DuAnId),
                "Không tồn tại dự án");

            using (await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken)) {
                var isExist = QuyetDinhDuyetDuAn.GetQueryableSet().Any(o => o.Id == request.Entity.Id);
                if (isExist) {
                    await HandleNguonVon(request.Entity, cancellationToken);
                } else {
                    ManagedException.ThrowIf(
                        QuyetDinhDuyetDuAn.GetQueryableSet().Any(e => e.DuAnId == request.Entity.DuAnId),
                        "Mỗi dự án chỉ có duy nhất 1 quyết định phê duyệt dự án");
                    await QuyetDinhDuyetDuAn.AddAsync(request.Entity, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }
        } catch (Exception ex) {
            _logger.LogError(ex, ex.Message);
            throw;
        }
    }


    private async Task HandleNguonVon(QuyetDinhDuyetDuAn entity, CancellationToken cancellationToken = default) {
        var requestObjs = entity.QuyetDinhDuyetDuAnNguonVons?
            .Where(e => e.NguonVonId > 0)
            .DistinctBy(e => e.NguonVonId)
            .ToList() ?? [];

        entity.QuyetDinhDuyetDuAnNguonVons = [];

        // Lấy existing từ DB
        var existing = await QuyetDinhDuyetDuAnNguonVon.GetQueryableSet()
            .AsNoTracking()
            .Where(e => e.QuyetDinhDuyetDuAnId == entity.Id)
            .ToListAsync(cancellationToken);

        var newIds = requestObjs.Select(e => e.Id).ToList();
        var existingIds = existing.Select(e => e.Id).ToHashSet();

        // Thêm mới
        var toAdd = requestObjs
            .Where(e => !existingIds.Contains(e.Id))
            .Select(e => new QuyetDinhDuyetDuAnNguonVon {
                Id = e.Id,
                QuyetDinhDuyetDuAnId = entity.Id,
                NguonVonId = e.NguonVonId,
                GiaTri = e.GiaTri
            })
            .ToList();
        if (toAdd.Count > 0)
            QuyetDinhDuyetDuAnNguonVon.BulkInsert(toAdd, true);

        // Xóa bỏ
        var toRemove = existing
            .Where(e => !newIds.Contains(e.Id))
            .ToList();
        if (toRemove.Count > 0)
            QuyetDinhDuyetDuAnNguonVon.BulkDelete(toRemove);

        // Cập nhật
        var toUpdate = existing
            .Where(e => newIds.Contains(e.Id))
            .ToList();
        if (toUpdate.Count > 0) {
            var updates = toUpdate
                .Select(item => {
                    var newData = requestObjs.First(e => e.Id == item.Id);
                    item.GiaTri = newData.GiaTri;
                    return item;
                })
                .ToList();

            QuyetDinhDuyetDuAnNguonVon.BulkUpdate(updates, e => new { e.GiaTri });
        }
    }
}