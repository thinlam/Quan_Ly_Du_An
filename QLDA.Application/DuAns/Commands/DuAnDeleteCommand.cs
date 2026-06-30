using System.Data;
using BuildingBlocks.CrossCutting.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QLDA.Application.Authorization;
using QLDA.Application.Common;
using QLDA.Application.Common.Constants;

namespace QLDA.Application.DuAns.Commands;

public record DuAnDeleteCommand(Guid Id) : IRequest;

internal class DuAnDeleteCommandHandler : IRequestHandler<DuAnDeleteCommand> {
    private readonly IRepository<DuAn, Guid> DuAn;
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem;
    private readonly IAuthorizationManager _auth;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DuAnDeleteCommandHandler> _logger;

    public DuAnDeleteCommandHandler(IServiceProvider serviceProvider,
        ILogger<DuAnDeleteCommandHandler> logger) {
        DuAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
        _auth = serviceProvider.GetRequiredService<IAuthorizationManager>();
        _logger = logger;
        _unitOfWork = DuAn.UnitOfWork;
    }

    public async Task Handle(DuAnDeleteCommand request, CancellationToken cancellationToken = default) {
        try {
            using (await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken)) {
                await DeleteHandler(request, cancellationToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }
        } catch (Exception ex) {
            _logger.LogError(ex, ex.Message);
            throw;
        }
    }
    /*
     * Cách 1: bật IsDeleted = true cho dự án và các bảng liên quan
     * Tạm thời không làm
     */
    // private async Task DeleteHandler(DuAnDeleteCommand request, CancellationToken cancellationToken = default) {
    //     var entity = await DuAn.GetOriginalSet()
    //         .Where(e => e.Id == request.Id)
    //         .FirstOrDefaultAsync(cancellationToken);
    //     ManagedException.ThrowIfNull(entity);
    // }

    /*
     * Cách 2: chỉ bật IsDeleted = true cho dự án sau đó, các dự liệu liên quan buôộc phải join kiểm tra dự án IsDeleted = false mới được lấy lên
     * Ưu: Xử lý code lúc xóa ít,
     * Nhược: giảm hiệu năng read khi select dữ liệu liên quan vì bắt buộc phải join với dự án
     */
    private async Task DeleteHandler(DuAnDeleteCommand request, CancellationToken cancellationToken = default) {
        var entity = await DuAn.GetOrderedSet()
            .Include(e => e.DuAnBuocs)
            .Include(e => e.VanBanQuyetDinhs)
            .Include(e => e.BaoCaos)
            .Include(e => e.GoiThaus)
            .Include(e => e.HopDongs)
            .Include(e => e.KetQuaTrungThaus)
            .Include(e => e.PhuLucHopDongs)
            .Include(e => e.NghiemThus)
            .Include(e => e.ThanhToans)
            .Include(e => e.TamUngs)
            .Include(e => e.DuToans)
            .Where(e => e.Id == request.Id)
            .FirstOrDefaultAsync(cancellationToken);

        ManagedException.ThrowIfNull(entity);

        // Ownership: chỉ Lãnh đạo phụ trách / Người tạo / Phòng ban phụ trách chính /
        // Phòng ban phối hợp (thuộc DuAnChiuTrachNhiemXuLys, Loai=DonViPhoiHop) mới được xóa.
        if (!await _auth.CanExecuteAsync(AuthorizationResourceKeys.DuAn, entity, cancellationToken))
            throw new ForbiddenException("User không có quyền xóa dự án này");

        // Check for related entities that prevent deletion
        ManagedException.ThrowIf(entity.VanBanQuyetDinhs != null && entity.VanBanQuyetDinhs.Any(e => !e.IsDeleted), DeleteMessageConstants.VanBanQuyetDinh);
        ManagedException.ThrowIf(entity.BaoCaos != null && entity.BaoCaos.Any(e => !e.IsDeleted), DeleteMessageConstants.BaoCao);
        ManagedException.ThrowIf(entity.GoiThaus != null && entity.GoiThaus.Any(e => !e.IsDeleted), DeleteMessageConstants.GoiThau);
        ManagedException.ThrowIf(entity.HopDongs != null && entity.HopDongs.Any(e => !e.IsDeleted), DeleteMessageConstants.HopDong);
        ManagedException.ThrowIf(entity.KetQuaTrungThaus != null && entity.KetQuaTrungThaus.Any(e => !e.IsDeleted), DeleteMessageConstants.KetQuaTrungThau);
        ManagedException.ThrowIf(entity.PhuLucHopDongs != null && entity.PhuLucHopDongs.Any(e => !e.IsDeleted), DeleteMessageConstants.PhuLucHopDong);
        ManagedException.ThrowIf(entity.NghiemThus != null && entity.NghiemThus.Any(e => !e.IsDeleted), DeleteMessageConstants.NghiemThu);
        ManagedException.ThrowIf(entity.ThanhToans != null && entity.ThanhToans.Any(e => !e.IsDeleted), DeleteMessageConstants.ThanhToan);
        ManagedException.ThrowIf(entity.TamUngs != null && entity.TamUngs.Any(e => !e.IsDeleted), DeleteMessageConstants.TamUng);
        ManagedException.ThrowIf(entity.DuToans != null && entity.DuToans.Any(e => !e.IsDeleted), DeleteMessageConstants.DuToan);

        // Set IsDeleted = true for the project and all related entities
        entity.IsDeleted = true;

        #region Trường hợp muốn xoá các bảng liên quan 
        foreach (var item in entity.DuAnBuocs!) item.IsDeleted = true;
        #endregion

        await SyncHelper.SetDeleteWithRelatedFiles(TepDinhKem, [entity.Id.ToString()], true, cancellationToken);
    }
}