using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.HoSoDeXuatCapDoCntts.DTOs;
using QLDA.Domain.Constants;

namespace QLDA.Application.HoSoDeXuatCapDoCntts.Commands;

public record HoSoDeXuatCapDoCnttInsertCommand(HoSoDeXuatCapDoCnttInsertDto Dto)
    : IRequest<HoSoDeXuatCapDoCntt>;

internal class HoSoDeXuatCapDoCnttInsertCommandHandler : IRequestHandler<HoSoDeXuatCapDoCnttInsertCommand, HoSoDeXuatCapDoCntt> {
    private readonly IRepository<HoSoDeXuatCapDoCntt, Guid> HoSoDeXuatCapDoCntt;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepo;
    private readonly IUnitOfWork _unitOfWork;

    public HoSoDeXuatCapDoCnttInsertCommandHandler(IServiceProvider serviceProvider) {
        HoSoDeXuatCapDoCntt = serviceProvider.GetRequiredService<IRepository<HoSoDeXuatCapDoCntt, Guid>>();
        _statusRepo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _unitOfWork = HoSoDeXuatCapDoCntt.UnitOfWork;
    }

    public async Task<HoSoDeXuatCapDoCntt> Handle(HoSoDeXuatCapDoCnttInsertCommand request, CancellationToken cancellationToken = default) {
        var trangThaiDuThao = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.HoSoDeXuatCapDoCntt.DuThao && s.Loai == PheDuyetEntityNames.HoSoDeXuatCapDoCntt, cancellationToken);

        var entity = request.Dto.ToEntity();
        entity.TrangThaiId = trangThaiDuThao?.Id;

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await HoSoDeXuatCapDoCntt.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return entity!;
    }
}