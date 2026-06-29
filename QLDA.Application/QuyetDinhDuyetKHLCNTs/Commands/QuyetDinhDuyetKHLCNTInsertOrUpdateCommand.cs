using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace QLDA.Application.QuyetDinhDuyetKHLCNTs.Commands;

public record QuyetDinhDuyetKHLCNTInsertOrUpdateCommand(QuyetDinhDuyetKHLCNT Entity) : IRequest {
}

internal class
    QuyetDinhDuyetKHLCNTInsertOrUpdateCommandHandler : IRequestHandler<QuyetDinhDuyetKHLCNTInsertOrUpdateCommand> {
    private readonly DbContext _dbContext;
    private readonly IRepository<KeHoachLuaChonNhaThau, Guid> KeHoachLuaChonNhaThau;
    private readonly IRepository<QuyetDinhDuyetKHLCNT, Guid> QuyetDinhDuyetKHLCNT;
    private readonly IRepository<DuAn, Guid> DuAn;
    private readonly IRepository<DanhMucBuoc, int> DanhMucBuoc;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<QuyetDinhDuyetKHLCNTInsertOrUpdateCommandHandler> _logger;

    public QuyetDinhDuyetKHLCNTInsertOrUpdateCommandHandler(DbContext dbContext, IServiceProvider serviceProvider,
        ILogger<QuyetDinhDuyetKHLCNTInsertOrUpdateCommandHandler> logger) {
        _dbContext = dbContext;
        QuyetDinhDuyetKHLCNT = serviceProvider.GetRequiredService<IRepository<QuyetDinhDuyetKHLCNT, Guid>>();
        KeHoachLuaChonNhaThau = serviceProvider.GetRequiredService<IRepository<KeHoachLuaChonNhaThau, Guid>>();
        DuAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
        DanhMucBuoc = serviceProvider.GetRequiredService<IRepository<DanhMucBuoc, int>>();
        _logger = logger;
        _unitOfWork = QuyetDinhDuyetKHLCNT.UnitOfWork;
    }

    public async Task Handle(QuyetDinhDuyetKHLCNTInsertOrUpdateCommand request,
        CancellationToken cancellationToken = default) {
        try {
            // 1. Tách hẳn ID ra biến riêng biệt, KHÔNG truyền nguyên cả Object Navigation vào câu query
            var targetDuAnId = request.Entity.VanBanQuyetDinh?.DuAnId;
            var targetQuyetDinhId = request.Entity.Id;

            // 2. Sử dụng AsNoTracking() cho câu check Any để đảm bảo tuyệt đối không sinh vết tracking ẩn
            var isDuAnExist = await DuAn.GetQueryableSet()
                                        .AsNoTracking()
                                        .AnyAsync(e => e.Id == targetDuAnId, cancellationToken);

            ManagedException.ThrowIf(!isDuAnExist, "Không tồn tại dự án");
            // ====================================================================
            // KHÚC SỬA QUAN TRỌNG: Chủ động gỡ bỏ tracking của ID trùng nếu có
            // Thay `_dbContext` bằng biến DbContext thực tế trong Project của bạn
            // ====================================================================
           
            _dbContext.ChangeTracker.Clear(); 
            using (await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken))
            {
                // Dùng biến targetQuyetDinhId đã tách riêng
                var isExist = await QuyetDinhDuyetKHLCNT.GetQueryableSet()
                                                       .AsNoTracking()
                                                       .AnyAsync(o => o.Id == targetQuyetDinhId, cancellationToken);
                if (isExist)
                {
                    // Load thực thể kèm theo thực thể con từ DB lên bằng ID riêng biệt
                    var dbEntity = await QuyetDinhDuyetKHLCNT.GetQueryableSet()
                        .Include(x => x.VanBanQuyetDinh)
                        .FirstOrDefaultAsync(x => x.Id == targetQuyetDinhId, cancellationToken);

                    if (dbEntity != null)
                    {
                        // Cập nhật các trường của thực thể cha
                        dbEntity.KeHoachLuaChonNhaThauId = request.Entity.KeHoachLuaChonNhaThauId;

                        // Cập nhật các trường của thực thể con VanBanQuyetDinh
                        if (request.Entity.VanBanQuyetDinh != null)
                        {
                            dbEntity.VanBanQuyetDinh.DuAnId = request.Entity.VanBanQuyetDinh.DuAnId;
                            dbEntity.VanBanQuyetDinh.BuocId = request.Entity.VanBanQuyetDinh.BuocId;
                            dbEntity.VanBanQuyetDinh.So = request.Entity.VanBanQuyetDinh.So;
                            dbEntity.VanBanQuyetDinh.Ngay = request.Entity.VanBanQuyetDinh.Ngay;
                            dbEntity.VanBanQuyetDinh.TrichYeu = request.Entity.VanBanQuyetDinh.TrichYeu;
                            dbEntity.VanBanQuyetDinh.CoQuanQuyetDinh = request.Entity.VanBanQuyetDinh.CoQuanQuyetDinh;
                            dbEntity.VanBanQuyetDinh.NguoiKy = request.Entity.VanBanQuyetDinh.NguoiKy;
                            dbEntity.VanBanQuyetDinh.NgayKy = request.Entity.VanBanQuyetDinh.NgayKy;
                        }

                        await _unitOfWork.SaveChangesAsync(cancellationToken);
                    }
                }
                else
                {
                    await QuyetDinhDuyetKHLCNT.AddAsync(request.Entity, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }

                await HandleKeHoachLuaChonNhaThau(request, cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw;
        }
    }

    private async Task HandleKeHoachLuaChonNhaThau(QuyetDinhDuyetKHLCNTInsertOrUpdateCommand request,
        CancellationToken cancellationToken = default) {
        var keHoachLuaChonNhaThau = await KeHoachLuaChonNhaThau.GetQueryableSet()
            .Include(e => e.GoiThaus)
            .FirstOrDefaultAsync(e => e.Id == request.Entity.KeHoachLuaChonNhaThauId, cancellationToken);
        ManagedException.ThrowIfNull(keHoachLuaChonNhaThau);
        if (keHoachLuaChonNhaThau.GoiThaus == null || keHoachLuaChonNhaThau.GoiThaus.Count == 0) return;
        // foreach (var item in keHoachLuaChonNhaThau.GoiThaus) {
        //     item.DaDuyet = true;
        // }
    }
}