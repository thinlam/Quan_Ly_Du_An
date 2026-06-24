using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.PhanQuyenChucNangs;
using QLDA.Application.Authorization;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.PhanQuyenChucNangs.Commands;

public record PhanQuyenChucNangUpdateCommand(PhanQuyenChucNang Dto) : IRequest<PhanQuyenChucNang>;

internal class PhanQuyenChucNangUpdateCommandHandler : IRequestHandler<PhanQuyenChucNangUpdateCommand, PhanQuyenChucNang>
{
    private readonly IRepository<PhanQuyenChucNang, int> _repo;
    private readonly IUnitOfWork _unitOfWork;

    public PhanQuyenChucNangUpdateCommandHandler(IServiceProvider serviceProvider)
    {
        _repo = serviceProvider.GetRequiredService<IRepository<PhanQuyenChucNang, int>>();
        _unitOfWork = _repo.UnitOfWork;
    }

    public async Task<PhanQuyenChucNang> Handle(PhanQuyenChucNangUpdateCommand request, CancellationToken cancellationToken = default)
    {

        var entity = await _repo.GetQueryableSet()
            
            .FirstOrDefaultAsync(e => e.Id == request.Dto.Id, cancellationToken);// Không tìm thấy dữ liệu
        ManagedException.ThrowIf(entity == null, "Không tìm thấy dữ liệu.");

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await _repo.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return entity;
    }
}
