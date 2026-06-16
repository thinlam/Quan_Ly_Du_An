using Azure.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QLDA.Application.Common;
using QLDA.Application.QuyetDinhDuyetDuToanDtos.DTOs;
using QLDA.Application.QuyetDinhDuyetDuToans.DTOs;
using QLDA.Domain.Constants;
using System.Data;

namespace QLDA.Application.QuyetDinhDuyetDuToans.Commands;

public record QuyetDinhDuyetDuToanUpdateCommand(QuyetDinhDuyetDuToanInsUpdDto Entity) : IRequest<QuyetDinhDuyetDuToan>;

internal class
    QuyetDinhDuyetDuToanUpdateCommandHandler : IRequestHandler<QuyetDinhDuyetDuToanUpdateCommand, QuyetDinhDuyetDuToan>
{
    private readonly IRepository<QuyetDinhDuyetDuToan, Guid> QuyetDinhDuyetDuToan;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepo;
    private readonly IRepository<DuAn, Guid> DuAn;
    private readonly IRepository<DanhMucBuoc, int> DanhMucBuoc;
    private readonly IUnitOfWork UnitOfWork;
    private readonly ILogger<QuyetDinhDuyetDuToanUpdateCommandHandler> Logger;

    public QuyetDinhDuyetDuToanUpdateCommandHandler(IServiceProvider serviceProvider,
        ILogger<QuyetDinhDuyetDuToanUpdateCommandHandler> logger)
    {
        QuyetDinhDuyetDuToan = serviceProvider.GetRequiredService<IRepository<QuyetDinhDuyetDuToan, Guid>>();
        DuAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
        _statusRepo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        DanhMucBuoc = serviceProvider.GetRequiredService<IRepository<DanhMucBuoc, int>>();
        Logger = logger;
        UnitOfWork = QuyetDinhDuyetDuToan.UnitOfWork;
    }
    public async Task<QuyetDinhDuyetDuToan> Handle(QuyetDinhDuyetDuToanUpdateCommand request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            ManagedException.ThrowIf(!DuAn.GetQueryableSet().Any(e => e.Id == request.Entity.DuAnId),
                "Không tồn tại dự án");

            var statuses = await _statusRepo.GetByLoaiAsync(PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);
            var statusDict = statuses.ToDictionary(x => x.Ma);

            var trangThaiDuThao = statusDict.GetValueOrDefault(TrangThaiPheDuyetCodes.DeXuatMacDinh.DuThao);
            var trangThaiTraLai = statusDict.GetValueOrDefault(TrangThaiPheDuyetCodes.DeXuatMacDinh.TraLai);

            var entity = await QuyetDinhDuyetDuToan.GetQueryableSet()
                .Include(e => e.ChiPhis)
                .Include(e => e.KeHoachVons)
                .FirstOrDefaultAsync(e => e.Id == request.Entity.Id, cancellationToken);

            ManagedException.ThrowIf(entity == null, "Không tìm thấy dữ liệu.");

            // Validate trạng thái
            if (entity.TrangThaiId != trangThaiDuThao?.Id && entity.TrangThaiId != trangThaiTraLai?.Id)
                throw new ManagedException("Trạng thái không thể cập nhật!");

            // Giữ lại trạng thái cũ giống logic của bạn
            var trangThaiIdOld = entity.TrangThaiId;

            request.Entity.UpdateToEntity(entity);

            entity.TrangThaiId = trangThaiIdOld;
            await UnitOfWork.SaveChangesAsync(cancellationToken);

            return entity;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, ex.Message);
            throw;
        }
    }
  

}