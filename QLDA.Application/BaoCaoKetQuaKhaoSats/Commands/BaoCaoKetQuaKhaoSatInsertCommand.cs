using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.BaoCaoKetQuaKhaoSats.DTOs;
using QLDA.Domain.Constants;

namespace QLDA.Application.BaoCaoKetQuaKhaoSats.Commands;

public record BaoCaoKetQuaKhaoSatInsertCommand(BaoCaoKetQuaKhaoSatInsertDto Dto)
    : IRequest<BaoCaoKetQuaKhaoSat>;

internal class BaoCaoKetQuaKhaoSatInsertCommandHandler
    : IRequestHandler<BaoCaoKetQuaKhaoSatInsertCommand, BaoCaoKetQuaKhaoSat>
{
    private readonly IRepository<BaoCaoKetQuaKhaoSat, Guid> _repository;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepo;
    private readonly IAuthorizationManager _authManager;
    private readonly IAuthorizationContext _authContext;
    private readonly IUnitOfWork _unitOfWork;

    public BaoCaoKetQuaKhaoSatInsertCommandHandler(IServiceProvider serviceProvider)
    {
        _repository = serviceProvider.GetRequiredService<IRepository<BaoCaoKetQuaKhaoSat, Guid>>();
        _statusRepo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _authManager = serviceProvider.GetRequiredService<IAuthorizationManager>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task<BaoCaoKetQuaKhaoSat> Handle(
        BaoCaoKetQuaKhaoSatInsertCommand request,
        CancellationToken cancellationToken = default)
    {
        await _authManager.EnsureCanExecuteAsync(request.Dto.BuocId, request.Dto.DuAnId, _authContext, cancellationToken);

        var trangThaiDuThao = await _statusRepo
            .GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s =>
                s.Ma == TrangThaiPheDuyetCodes.DeXuatMacDinh.DuThao &&
                s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);

        var entity = request.Dto.ToEntity();
        entity.TrangThaiId = trangThaiDuThao?.Id;

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return entity!;
    }
}
