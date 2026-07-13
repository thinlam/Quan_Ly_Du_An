using System.Data;
using Microsoft.Extensions.Logging;
using QLDA.Application.Authorization;
using QLDA.Application.QuyetDinhDuyetDuAns.DTOs;
using QLDA.Application.QuyetDinhDuyetDuAnNguonVons.DTOs;

namespace QLDA.Application.QuyetDinhDuyetDuAns.Commands;

public record QuyetDinhDuyetDuAnInsertCommand(QuyetDinhDuyetDuAnInsertDto Dto) : IRequest<QuyetDinhDuyetDuAn>;

internal class QuyetDinhDuyetDuAnInsertCommandHandler : IRequestHandler<QuyetDinhDuyetDuAnInsertCommand, QuyetDinhDuyetDuAn> {
    private readonly IRepository<DuAn, Guid> DuAn;
    private readonly IRepository<DanhMucBuoc, int> DanhMucBuoc;
    private readonly IRepository<QuyetDinhDuyetDuAn, Guid> QuyetDinhDuyetDuAn;
    private readonly IRepository<QuyetDinhDuyetDuAnNguonVon, Guid> QuyetDinhDuyetDuAnNguonVon;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<QuyetDinhDuyetDuAnInsertCommandHandler> _logger;
    private readonly IAuthorizationManager _authManager;
    private readonly IAuthorizationContext _authContext;

    public QuyetDinhDuyetDuAnInsertCommandHandler(IServiceProvider serviceProvider,
        ILogger<QuyetDinhDuyetDuAnInsertCommandHandler> logger) {
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

    public async Task<QuyetDinhDuyetDuAn> Handle(QuyetDinhDuyetDuAnInsertCommand request, CancellationToken cancellationToken = default) {
        try {
            await _authManager.EnsureCanExecuteAsync(request.Dto.BuocId, request.Dto.DuAnId, _authContext, cancellationToken);
            ManagedException.ThrowIf(!DuAn.GetQueryableSet().Any(e => e.Id == request.Dto.DuAnId),
                "Không tồn tại dự án");

            ManagedException.ThrowIf(
                QuyetDinhDuyetDuAn.GetQueryableSet().Any(e => e.DuAnId == request.Dto.DuAnId),
                "Mỗi dự án chỉ có duy nhất 1 quyết định phê duyệt dự án");

            var entity = request.Dto.ToEntity();

            using (await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken)) {
                await QuyetDinhDuyetDuAn.AddAsync(entity, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await HandleNguonVon(entity, request.Dto.NguonVons, cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }

            return entity;
        } catch (Exception ex) {
            _logger.LogError(ex, ex.Message);
            throw;
        }
    }

    private async Task HandleNguonVon(QuyetDinhDuyetDuAn entity, List<QuyetDinhDuyetDuAnNguonVonDto>? nguonVons, CancellationToken cancellationToken = default) {
        if (nguonVons == null || nguonVons.Count == 0) return;

        var toAdd = nguonVons
            .Where(e => e.NguonVonId > 0)
            .DistinctBy(e => e.NguonVonId)
            .Select(e => new QuyetDinhDuyetDuAnNguonVon {
                Id = Guid.NewGuid(),
                QuyetDinhDuyetDuAnId = entity.Id,
                NguonVonId = e.NguonVonId,
                GiaTri = e.GiaTri
            })
            .ToList();

        if (toAdd.Count > 0) {
            await QuyetDinhDuyetDuAnNguonVon.AddRangeAsync(toAdd, cancellationToken);
        }
    }
}