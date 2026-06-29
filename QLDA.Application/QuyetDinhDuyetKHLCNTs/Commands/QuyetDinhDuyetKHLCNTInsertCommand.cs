using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QLDA.Application.Common.Constants;
using QLDA.Application.QuanLyPheDuyet.DTOs;
using QLDA.Application.QuyetDinhDuyetKHLCNTs.DTOs;
using QLDA.Domain.Constants;
using System.Data;

namespace QLDA.Application.QuyetDinhDuyetKHLCNTs.Commands;

public record QuyetDinhDuyetKHLCNTInsertCommand(QuyetDinhDuyetKHLCNTInsertDto Dto) : IRequest<QuyetDinhDuyetKHLCNT>;

internal class QuyetDinhDuyetKHLCNTInsertCommandHandler : IRequestHandler<QuyetDinhDuyetKHLCNTInsertCommand, QuyetDinhDuyetKHLCNT> {
    private readonly IRepository<KeHoachLuaChonNhaThau, Guid> KeHoachLuaChonNhaThau;
    private readonly IRepository<QuyetDinhDuyetKHLCNT, Guid> QuyetDinhDuyetKHLCNT;
    private readonly IRepository<DuAn, Guid> DuAn;
    private readonly IRepository<DanhMucBuoc, int> DanhMucBuoc;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<QuyetDinhDuyetKHLCNTInsertCommandHandler> _logger;

    public QuyetDinhDuyetKHLCNTInsertCommandHandler(IServiceProvider serviceProvider,
        ILogger<QuyetDinhDuyetKHLCNTInsertCommandHandler> logger) {
        QuyetDinhDuyetKHLCNT = serviceProvider.GetRequiredService<IRepository<QuyetDinhDuyetKHLCNT, Guid>>();
        DuAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
        DanhMucBuoc = serviceProvider.GetRequiredService<IRepository<DanhMucBuoc, int>>();
        _logger = logger;
        _unitOfWork = QuyetDinhDuyetKHLCNT.UnitOfWork;
    }

    public async Task<QuyetDinhDuyetKHLCNT> Handle(QuyetDinhDuyetKHLCNTInsertCommand request, CancellationToken cancellationToken = default) {
        try {
            ManagedException.ThrowIf(!DuAn.GetQueryableSet().Any(e => e.Id == request.Dto.DuAnId),
                "Không tồn tại dự án");

            var entity = request.Dto.ToEntity();
            // 2. Khởi tạo đồng thời bản ghi cha VanBanQuyetDinh từ thông tin request
            entity.VanBanQuyetDinh = new VanBanQuyetDinh
            {
                Id = Guid.NewGuid(),
                DuAnId = request.Dto.DuAnId,
                BuocId = request.Dto.BuocId,
                So = request.Dto.SoQuyetDinh, 
                Ngay = request.Dto.NgayQuyetDinh,
                TrichYeu = request.Dto.TrichYeu,
                CoQuanQuyetDinh = request.Dto.CoQuanQuyetDinh,
                Loai = LoaiVanBanQuyetDinhConst.QuyetDinhDuyetKHLCNT
            };//
            using (await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken)) {
                await QuyetDinhDuyetKHLCNT.AddAsync(entity, cancellationToken);
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