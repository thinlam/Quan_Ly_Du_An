using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.BaoCaoKetQuaKhaoSats.DTOs;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.BaoCaoKetQuaKhaoSats.Commands;

public record BaoCaoKetQuaKhaoSatInsertCommand(BaoCaoKetQuaKhaoSatInsertDto Dto)
    : IRequest<BaoCaoKetQuaKhaoSat>;

internal class BaoCaoKetQuaKhaoSatInsertCommandHandler
    : IRequestHandler<BaoCaoKetQuaKhaoSatInsertCommand, BaoCaoKetQuaKhaoSat>
{
    private readonly IRepository<BaoCaoKetQuaKhaoSat, Guid> _repository;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepo;
    private readonly IUnitOfWork _unitOfWork;

    public BaoCaoKetQuaKhaoSatInsertCommandHandler(IServiceProvider serviceProvider)
    {
        _repository = serviceProvider.GetRequiredService<IRepository<BaoCaoKetQuaKhaoSat, Guid>>();
        _statusRepo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task<BaoCaoKetQuaKhaoSat> Handle(
        BaoCaoKetQuaKhaoSatInsertCommand request,
        CancellationToken cancellationToken = default)
    {
        var trangThaiDuThao = await _statusRepo
            .GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s =>
                s.Ma == TrangThaiPheDuyetCodes.BaoCaoKetQuaKhaoSat.DuThao &&
                s.Loai == PheDuyetEntityNames.BaoCaoKetQuaKhaoSat, cancellationToken);

        var entity = request.Dto.ToEntity();
        entity.TrangThaiId = trangThaiDuThao?.Id;

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return entity;
    }
}
