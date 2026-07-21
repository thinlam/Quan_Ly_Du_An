using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Domain.Constants;

namespace QLDA.Application.ThoaThuanGiaoViecs.Commands;

public record ThoaThuanGiaoViecInsertCommand(ThoaThuanGiaoViec Dto) : IRequest<ThoaThuanGiaoViec>;

internal class ThoaThuanGiaoViecInsertCommandHandler : IRequestHandler<ThoaThuanGiaoViecInsertCommand, ThoaThuanGiaoViec>
{
    private readonly IRepository<ThoaThuanGiaoViec, Guid> _repo;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepo;
    private readonly IAuthorizationManager _authManager;
    private readonly IAuthorizationContext _authContext;
    private readonly IUnitOfWork _unitOfWork;

    public ThoaThuanGiaoViecInsertCommandHandler(IServiceProvider serviceProvider)
    {
        _repo = serviceProvider.GetRequiredService<IRepository<ThoaThuanGiaoViec, Guid>>();
        _statusRepo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _authManager = serviceProvider.GetRequiredService<IAuthorizationManager>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _unitOfWork = _repo.UnitOfWork;
    }

    public async Task<ThoaThuanGiaoViec> Handle(ThoaThuanGiaoViecInsertCommand request, CancellationToken cancellationToken = default)
    {
        await _authManager.EnsureCanExecuteAsync(request.Dto.BuocId, request.Dto.DuAnId, _authContext, cancellationToken);

        // Auto-assign Dự thảo status
        var trangThaiDuThao = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == "DT" && s.Loai == PheDuyetEntityNames.ThoaThuanGiaoViec, cancellationToken);

        request.Dto!.TrangThaiId = trangThaiDuThao!.Id;
        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await _repo.AddAsync(request.Dto, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return request.Dto;
    }
}
