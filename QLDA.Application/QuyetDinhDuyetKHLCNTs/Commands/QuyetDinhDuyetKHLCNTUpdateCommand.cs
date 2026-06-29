using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QLDA.Application.Common.Constants;
using QLDA.Application.QuyetDinhDuyetKHLCNTs.DTOs;
using System.Data;

namespace QLDA.Application.QuyetDinhDuyetKHLCNTs.Commands;

public record QuyetDinhDuyetKHLCNTUpdateCommand(QuyetDinhDuyetKHLCNTUpdateDto Dto) : IRequest<QuyetDinhDuyetKHLCNT>;

internal class QuyetDinhDuyetKHLCNTUpdateCommandHandler : IRequestHandler<QuyetDinhDuyetKHLCNTUpdateCommand, QuyetDinhDuyetKHLCNT> {
    private readonly IRepository<KeHoachLuaChonNhaThau, Guid> KeHoachLuaChonNhaThau;
    private readonly IRepository<QuyetDinhDuyetKHLCNT, Guid> QuyetDinhDuyetKHLCNT;
    private readonly IRepository<DuAn, Guid> DuAn;
    private readonly IRepository<DanhMucBuoc, int> DanhMucBuoc;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<QuyetDinhDuyetKHLCNTUpdateCommandHandler> _logger;

    public QuyetDinhDuyetKHLCNTUpdateCommandHandler(IServiceProvider serviceProvider,
        ILogger<QuyetDinhDuyetKHLCNTUpdateCommandHandler> logger) {
        QuyetDinhDuyetKHLCNT = serviceProvider.GetRequiredService<IRepository<QuyetDinhDuyetKHLCNT, Guid>>();
        KeHoachLuaChonNhaThau = serviceProvider.GetRequiredService<IRepository<KeHoachLuaChonNhaThau, Guid>>();
        DuAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
        DanhMucBuoc = serviceProvider.GetRequiredService<IRepository<DanhMucBuoc, int>>();
        _logger = logger;
        _unitOfWork = QuyetDinhDuyetKHLCNT.UnitOfWork;
    }

    public async Task<QuyetDinhDuyetKHLCNT> Handle(QuyetDinhDuyetKHLCNTUpdateCommand request, CancellationToken cancellationToken = default) {
        try {
            var entity = request.Dto.ToEntity();
            entity.VanBanQuyetDinh.Loai = LoaiVanBanQuyetDinhConst.QuyetDinhDuyetKHLCNT;

            using (await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken)) {
                await QuyetDinhDuyetKHLCNT.UpdateAsync(entity, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await HandleKeHoachLuaChonNhaThau(entity, cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }

            return entity;
        } catch (Exception ex) {
            _logger.LogError(ex, ex.Message);
            throw;
        }
    }

    private async Task HandleKeHoachLuaChonNhaThau(QuyetDinhDuyetKHLCNT entity, CancellationToken cancellationToken = default) {
        var keHoachLuaChonNhaThau = await KeHoachLuaChonNhaThau.GetQueryableSet()
            .Include(e => e.GoiThaus)
            .FirstOrDefaultAsync(e => e.Id == entity.KeHoachLuaChonNhaThauId, cancellationToken);
        ManagedException.ThrowIfNull(keHoachLuaChonNhaThau);
        if (keHoachLuaChonNhaThau.GoiThaus == null || keHoachLuaChonNhaThau.GoiThaus.Count == 0) return;
        // foreach (var item in keHoachLuaChonNhaThau.GoiThaus) {
        //     item.DaDuyet = true;
        // }
    }
}