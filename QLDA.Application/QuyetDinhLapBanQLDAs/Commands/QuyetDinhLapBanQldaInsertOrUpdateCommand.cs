using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QLDA.Application.Common;
using QLDA.Domain.Constants;
using System.Data;

namespace QLDA.Application.QuyetDinhLapBanQLDAs.Commands;

public record QuyetDinhLapBanQldaInsertOrUpdateCommand(QuyetDinhLapBanQLDA Entity) : IRequest {
}

internal class
    QuyetDinhLapBanQldaInsertOrUpdateCommandHandler : IRequestHandler<QuyetDinhLapBanQldaInsertOrUpdateCommand> {
    private readonly IRepository<QuyetDinhLapBanQLDA, Guid> QuyetDinhLapBanQLDA;
    private readonly IRepository<ThanhVienBanQLDA, int> ThanhVienBanQLDA;
    private readonly IRepository<DuAn, Guid> DuAn;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> StatusRepo;
    private readonly IRepository<DanhMucBuoc, int> DanhMucBuoc;
    private readonly IUnitOfWork UnitOfWork;
    private readonly ILogger<QuyetDinhLapBanQldaInsertOrUpdateCommandHandler> Logger;

    public QuyetDinhLapBanQldaInsertOrUpdateCommandHandler(IServiceProvider serviceProvider,
        ILogger<QuyetDinhLapBanQldaInsertOrUpdateCommandHandler> logger) {
        QuyetDinhLapBanQLDA = serviceProvider.GetRequiredService<IRepository<QuyetDinhLapBanQLDA, Guid>>();
        ThanhVienBanQLDA = serviceProvider.GetRequiredService<IRepository<ThanhVienBanQLDA, int>>();
        DuAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
        StatusRepo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        DanhMucBuoc = serviceProvider.GetRequiredService<IRepository<DanhMucBuoc, int>>();
        Logger = logger;
        UnitOfWork = QuyetDinhLapBanQLDA.UnitOfWork;
    }

    public async Task Handle(QuyetDinhLapBanQldaInsertOrUpdateCommand request,
        CancellationToken cancellationToken = default) {
        try {
            ManagedException.ThrowIf(!DuAn.GetQueryableSet().Any(e => e.Id == request.Entity.DuAnId),
                "Không tồn tại dự án");

            var statuses = await StatusRepo.GetByLoaiAsync(PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);
            var statusDict = statuses
                .Where(x => !string.IsNullOrWhiteSpace(x.Ma))
                .ToDictionary(x => x.Ma!, x => x);

            var trangThaiDuThao = statusDict.GetValueOrDefault(TrangThaiPheDuyetCodes.DeXuatMacDinh.DuThao);
            request.Entity.TrangThaiId = trangThaiDuThao?.Id??0;

            using (await UnitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken)) {
                var isExist = QuyetDinhLapBanQLDA.GetQueryableSet().Any(o => o.Id == request.Entity.Id);
                if (isExist) {
                    await HandleThanhVien(request.Entity, cancellationToken);
                } else {
                    await QuyetDinhLapBanQLDA.AddAsync(request.Entity, cancellationToken);
                    await UnitOfWork.SaveChangesAsync(cancellationToken);
                }

                await UnitOfWork.SaveChangesAsync(cancellationToken);
                await UnitOfWork.CommitTransactionAsync(cancellationToken);
            }
        } catch (Exception ex) {
            Logger.LogError(ex, ex.Message);
            throw;
        }
    }

    private async Task HandleThanhVien(QuyetDinhLapBanQLDA entity, CancellationToken cancellationToken = default) {
        var requestObjs = entity.ThanhViens.ToList();
        entity.ThanhViens = [];

        // Lấy existing từ DB
        var existing = await ThanhVienBanQLDA.GetQueryableSet()
            .AsNoTracking()
            .Where(e => e.QuyetDinhId == entity.Id)
            .ToListAsync(cancellationToken);

        var newIds = requestObjs.Select(e => e.Id).ToList();
        var existingIds = existing.Select(e => e.Id).ToHashSet();

        // Thêm mới
        var toAdd = requestObjs
            .Where(e => !existingIds.Contains(e.Id))
            .Select(e => new ThanhVienBanQLDA {
                Id = e.Id,
                QuyetDinhId = entity.Id,
                Ten = e.Ten,
                ChucVu = e.ChucVu,
                VaiTro = e.VaiTro,
            })
            .ToList();
        if (toAdd.Count > 0)
            ThanhVienBanQLDA.BulkInsert(toAdd);

        // Xóa bỏ
        var toRemove = existing
            .Where(e => !newIds.Contains(e.Id))
            .ToList();
        if (toRemove.Count > 0)
            ThanhVienBanQLDA.BulkDelete(toRemove);

        // Cập nhật
        var toUpdate = existing
            .Where(e => newIds.Contains(e.Id))
            .ToList();
        if (toUpdate.Count > 0) {
            var updates = toUpdate
                .Select(item => {
                    var newData = requestObjs.First(e => e.Id == item.Id);
                    item.Ten = newData.Ten;
                    item.ChucVu = newData.ChucVu;
                    item.VaiTro = newData.VaiTro;
                    return item;
                })
                .ToList();

            ThanhVienBanQLDA.BulkUpdate(updates, e => new { e.Ten, e.VaiTro, e.ChucVu });
        }
    }
}