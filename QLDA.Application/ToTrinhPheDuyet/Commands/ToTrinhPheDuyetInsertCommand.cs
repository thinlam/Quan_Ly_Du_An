using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.ToTrinhPheDuyets.DTOs;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.ToTrinhPheDuyets.Commands;

public record ToTrinhPheDuyetInsertCommand(ToTrinhPheDuyetInsertDto Dto) : IRequest<ToTrinhPheDuyet>;

internal class ToTrinhPheDuyetInsertCommandHandler : IRequestHandler<ToTrinhPheDuyetInsertCommand, ToTrinhPheDuyet>
{
    private readonly IRepository<ToTrinhPheDuyet, Guid> _repo;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepo;
    private readonly IUnitOfWork _unitOfWork;

    public ToTrinhPheDuyetInsertCommandHandler(IServiceProvider serviceProvider)
    {
        _repo = serviceProvider.GetRequiredService<IRepository<ToTrinhPheDuyet, Guid>>();
        _statusRepo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _unitOfWork = _repo.UnitOfWork;
    }

    public async Task<ToTrinhPheDuyet> Handle(ToTrinhPheDuyetInsertCommand request, CancellationToken cancellationToken = default)
    {
        // Auto-assign Dự thảo status
        var trangThaiDuThao = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == "DT" && s.Loai == PheDuyetEntityNames.ToTrinhPheDuyet, cancellationToken);

        var entity = new ToTrinhPheDuyet
        {
            DuAnId = request.Dto.DuAnId,
            BuocId = request.Dto.BuocId,
            So = request.Dto.So,
            NgayToTrinh = request.Dto.NgayToTrinh,
            TrichYeu = request.Dto.TrichYeu,
            TrangThaiId = trangThaiDuThao?.Id,
        };

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await _repo.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return entity;
    }
}
