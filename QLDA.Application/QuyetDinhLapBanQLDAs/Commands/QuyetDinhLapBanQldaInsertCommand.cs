using Microsoft.Extensions.Logging;
using QLDA.Application.Authorization;
using QLDA.Application.Common;
using QLDA.Application.QuyetDinhLapBanQLDAs.DTOs;
using QLDA.Domain.Constants;
using System.Data;

namespace QLDA.Application.QuyetDinhLapBanQLDAs.Commands;

public record QuyetDinhLapBanQldaInsertCommand(QuyetDinhLapBanQldaInsertDto Dto) : IRequest<QuyetDinhLapBanQLDA>;

internal class QuyetDinhLapBanQldaInsertCommandHandler : IRequestHandler<QuyetDinhLapBanQldaInsertCommand, QuyetDinhLapBanQLDA> {
    private readonly IRepository<QuyetDinhLapBanQLDA, Guid> QuyetDinhLapBanQLDA;
    private readonly IRepository<ThanhVienBanQLDA, int> ThanhVienBanQLDA;
    private readonly IRepository<DuAn, Guid> DuAn;
    private readonly IRepository<DanhMucBuoc, int> DanhMucBuoc;
    private readonly IUnitOfWork UnitOfWork;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _StatusRepository;
    private readonly ILogger<QuyetDinhLapBanQldaInsertCommandHandler> Logger;
    private readonly IAuthorizationManager _authManager;
    private readonly IAuthorizationContext _authContext;

    public QuyetDinhLapBanQldaInsertCommandHandler(IServiceProvider serviceProvider,
        ILogger<QuyetDinhLapBanQldaInsertCommandHandler> logger) {
        QuyetDinhLapBanQLDA = serviceProvider.GetRequiredService<IRepository<QuyetDinhLapBanQLDA, Guid>>();
        ThanhVienBanQLDA = serviceProvider.GetRequiredService<IRepository<ThanhVienBanQLDA, int>>();
        _StatusRepository = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        DuAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
        DanhMucBuoc = serviceProvider.GetRequiredService<IRepository<DanhMucBuoc, int>>();
        Logger = logger;
        UnitOfWork = QuyetDinhLapBanQLDA.UnitOfWork;
        _authManager = serviceProvider.GetRequiredService<IAuthorizationManager>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
    }

    public async Task<QuyetDinhLapBanQLDA> Handle(QuyetDinhLapBanQldaInsertCommand request, CancellationToken cancellationToken = default) {
        await _authManager.EnsureCanExecuteAsync(request.Dto.BuocId, request.Dto.DuAnId, _authContext, cancellationToken);

        try {
            ManagedException.ThrowIf(!DuAn.GetQueryableSet().Any(e => e.Id == request.Dto.DuAnId),
                "Không tồn tại dự án");

            var statuses = await _StatusRepository.GetByLoaiAsync(PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);
            var statusDict = statuses
                .Where(x => !string.IsNullOrWhiteSpace(x.Ma))
                .ToDictionary(x => x.Ma!, x => x);
            var trangThaiDuThao = statusDict.GetValueOrDefault(TrangThaiPheDuyetCodes.DeXuatMacDinh.DuThao);
            ManagedException.ThrowIfNull(trangThaiDuThao, "Không tìm thấy trạng thái 'Dự thảo'");

            var entity = request.Dto.ToEntity();
            entity.TrangThaiId = trangThaiDuThao?.Id;

            using (await UnitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken)) {
                await QuyetDinhLapBanQLDA.AddAsync(entity, cancellationToken);
                await UnitOfWork.SaveChangesAsync(cancellationToken);

                // Handle ThanhVienBanQlda
                if (entity.ThanhViens != null && entity.ThanhViens.Count > 0) {
                    foreach (var tv in entity.ThanhViens) {
                        tv.QuyetDinhId = entity.Id;
                    }
                    ThanhVienBanQLDA.BulkInsert(entity.ThanhViens);
                }

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