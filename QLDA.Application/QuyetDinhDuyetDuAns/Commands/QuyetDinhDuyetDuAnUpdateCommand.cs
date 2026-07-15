using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QLDA.Application.Authorization;
using QLDA.Application.QuyetDinhDuyetDuAns.DTOs;
using QLDA.Application.QuyetDinhDuyetDuAnNguonVons.DTOs;

namespace QLDA.Application.QuyetDinhDuyetDuAns.Commands;

public record QuyetDinhDuyetDuAnUpdateCommand(QuyetDinhDuyetDuAnUpdateDto Dto) : IRequest<QuyetDinhDuyetDuAn>;

internal class QuyetDinhDuyetDuAnUpdateCommandHandler : IRequestHandler<QuyetDinhDuyetDuAnUpdateCommand, QuyetDinhDuyetDuAn> {
    private readonly IRepository<DuAn, Guid> DuAn;
    private readonly IRepository<DanhMucBuoc, int> DanhMucBuoc;
    private readonly IRepository<QuyetDinhDuyetDuAn, Guid> QuyetDinhDuyetDuAn;
    private readonly IRepository<QuyetDinhDuyetDuAnNguonVon, Guid> QuyetDinhDuyetDuAnNguonVon;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<QuyetDinhDuyetDuAnUpdateCommandHandler> _logger;
    private readonly IAuthorizationManager _authManager;
    private readonly IAuthorizationContext _authContext;

    public QuyetDinhDuyetDuAnUpdateCommandHandler(IServiceProvider serviceProvider,
        ILogger<QuyetDinhDuyetDuAnUpdateCommandHandler> logger) {
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

    public async Task<QuyetDinhDuyetDuAn> Handle(QuyetDinhDuyetDuAnUpdateCommand request, CancellationToken cancellationToken = default) {
        try {

            var entity = await QuyetDinhDuyetDuAn.GetQueryableSet().AsNoTracking()
           .FirstOrDefaultAsync(e => e.Id == request.Dto.Id, cancellationToken);
            await _authManager.EnsureCanExecuteAsync(entity!.BuocId, entity.DuAnId, _authContext, cancellationToken);
            request.Dto.ToEntity(entity);

            using (await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken)) {
                await QuyetDinhDuyetDuAn.UpdateAsync(entity, cancellationToken);
                await HandleNguonVon(entity, request.Dto.NguonVons, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }

            return entity;
        } catch (Exception ex) {
            _logger.LogError(ex, ex.Message);
            throw;
        }
    }

    private async Task HandleNguonVon(QuyetDinhDuyetDuAn entity, List<QuyetDinhDuyetDuAnNguonVonDto>? nguonVons, CancellationToken cancellationToken = default) {
        var requestObjs = nguonVons?
            .Where(e => e.NguonVonId > 0)
            .DistinctBy(e => e.NguonVonId)
            .ToList() ?? [];

        // Lấy existing từ DB
        var existing = await QuyetDinhDuyetDuAnNguonVon.GetQueryableSet()
            .AsNoTracking()
            .Where(e => e.QuyetDinhDuyetDuAnId == entity.Id)
            .ToListAsync(cancellationToken);

        var newIds = requestObjs.Where(e => e.Id.HasValue).Select(e => e.Id!.Value).ToList();
        var existingIds = existing.Select(e => e.Id).ToHashSet();

        // Thêm mới
        var toAdd = requestObjs
            .Where(e => e.Id.HasValue && !existingIds.Contains(e.Id.Value))
            .Select(e => new QuyetDinhDuyetDuAnNguonVon {
                Id = e.Id!.Value,
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
