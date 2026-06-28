using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QLDA.Application.Common;
using QLDA.Application.QuyetDinhLapBanQLDAs.DTOs;
using QLDA.Domain.Constants;
using System.Data;

namespace QLDA.Application.QuyetDinhLapBanQLDAs.Commands;

public record QuyetDinhLapBanQldaUpdateCommand(QuyetDinhLapBanQldaUpdateDto Dto) : IRequest<QuyetDinhLapBanQLDA>;

internal class QuyetDinhLapBanQldaUpdateCommandHandler : IRequestHandler<QuyetDinhLapBanQldaUpdateCommand, QuyetDinhLapBanQLDA> {
    private readonly IRepository<QuyetDinhLapBanQLDA, Guid> QuyetDinhLapBanQLDA;
    private readonly IRepository<ThanhVienBanQLDA, int> ThanhVienBanQLDA;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> StatusRepo;
    private readonly IRepository<DuAn, Guid> DuAn;
    private readonly IRepository<DanhMucBuoc, int> DanhMucBuoc;
    private readonly IUnitOfWork UnitOfWork;
    private readonly ILogger<QuyetDinhLapBanQldaUpdateCommandHandler> Logger;

    public QuyetDinhLapBanQldaUpdateCommandHandler(IServiceProvider serviceProvider,
        ILogger<QuyetDinhLapBanQldaUpdateCommandHandler> logger) {
        QuyetDinhLapBanQLDA = serviceProvider.GetRequiredService<IRepository<QuyetDinhLapBanQLDA, Guid>>();
        ThanhVienBanQLDA = serviceProvider.GetRequiredService<IRepository<ThanhVienBanQLDA, int>>();
        DuAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
        DanhMucBuoc = serviceProvider.GetRequiredService<IRepository<DanhMucBuoc, int>>();
        StatusRepo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        Logger = logger;
        UnitOfWork = QuyetDinhLapBanQLDA.UnitOfWork;
    }

    public async Task<QuyetDinhLapBanQLDA> Handle(QuyetDinhLapBanQldaUpdateCommand request, CancellationToken cancellationToken = default) {
        try {
            var entity = await QuyetDinhLapBanQLDA.GetQueryableSet().AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == request.Dto.Id, cancellationToken);
            ManagedException.ThrowIf(entity == null, "Không tìm thấy dữ liệu.");
          
            var entityUpd = request.Dto.ToEntity();
            entityUpd.TrangThaiId = entity.TrangThaiId;

            var statuses = await StatusRepo.GetByLoaiAsync(PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);
            var statusDict = statuses
                .Where(x => !string.IsNullOrWhiteSpace(x.Ma))
                .ToDictionary(x => x.Ma!, x => x);

            var trangThaiDuThao = statusDict.GetValueOrDefault(TrangThaiPheDuyetCodes.DeXuatMacDinh.DuThao);
            var trangThaiTraLai = statusDict.GetValueOrDefault(TrangThaiPheDuyetCodes.DeXuatMacDinh.TraLai);
            if (entity.TrangThaiId != trangThaiDuThao?.Id && entity.TrangThaiId != trangThaiTraLai?.Id)
            {
                throw new ManagedException("Trạng thái không thể cập nhật!");
            }


            using (await UnitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken)) {
                await HandleThanhVien(entity, cancellationToken);
                await QuyetDinhLapBanQLDA.UpdateAsync(entity, cancellationToken);
                await UnitOfWork.SaveChangesAsync(cancellationToken);
                await UnitOfWork.CommitTransactionAsync(cancellationToken);
            }

            return entity;
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