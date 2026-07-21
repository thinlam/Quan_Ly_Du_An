using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.KeHoachLuaChonNhaThaus.DTOs;

namespace QLDA.Application.KeHoachLuaChonNhaThaus.Commands;

public record KeHoachLuaChonNhaThauInsertCommand(KeHoachLuaChonNhaThauInsertDto Dto) : IRequest<KeHoachLuaChonNhaThau>;

internal class KeHoachLuaChonNhaThauInsertCommandHandler : IRequestHandler<KeHoachLuaChonNhaThauInsertCommand, KeHoachLuaChonNhaThau> {
    private readonly IRepository<KeHoachLuaChonNhaThau, Guid> KeHoachLuaChonNhaThau;
    private readonly IRepository<DuAn, Guid> DuAn;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthorizationManager _authManager;
    private readonly IAuthorizationContext _authContext;

    public KeHoachLuaChonNhaThauInsertCommandHandler(IServiceProvider serviceProvider) {
        KeHoachLuaChonNhaThau = serviceProvider.GetRequiredService<IRepository<KeHoachLuaChonNhaThau, Guid>>();
        DuAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
        _unitOfWork = KeHoachLuaChonNhaThau.UnitOfWork;
        _authManager = serviceProvider.GetRequiredService<IAuthorizationManager>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
    }

    public async Task<KeHoachLuaChonNhaThau> Handle(KeHoachLuaChonNhaThauInsertCommand request, CancellationToken cancellationToken = default) {
        await _authManager.EnsureCanExecuteAsync(request.Dto.BuocId, request.Dto.DuAnId, _authContext, cancellationToken);

        await ValidateAsync(request, cancellationToken);

        var entity = request.Dto.ToEntity();
        entity.LoaiKeHoach = Domain.Constants.KeHoachLuaChonNhaThauLoai.KeHoachLCNT;
        if (_unitOfWork.HasTransaction) {
            await InsertAsync(entity, cancellationToken);
        } else {
            using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken); await InsertAsync(entity, cancellationToken);
            await InsertAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }


        return entity!;

    }

    #region  Private helper methods

    private async Task ValidateAsync(KeHoachLuaChonNhaThauInsertCommand request, CancellationToken cancellationToken) {
        ManagedException.ThrowIf(!await DuAn.GetQueryableSet().AnyAsync(e => e.Id == request.Dto.DuAnId, cancellationToken: cancellationToken),
           "Không tồn tại dự án");
        ManagedException.ThrowIf(
            when: await KeHoachLuaChonNhaThau.GetQueryableSet().AnyAsync(e => e.DuAnId == request.Dto.DuAnId && e.So == request.Dto.SoQuyetDinh && !e.IsDeleted, cancellationToken: cancellationToken),
            message: "Số quyết định đã tồn tại"
        );
    }

    private async Task InsertAsync(KeHoachLuaChonNhaThau entity, CancellationToken cancellationToken) {
        await KeHoachLuaChonNhaThau.AddAsync(entity, cancellationToken);
    }

    #endregion
}