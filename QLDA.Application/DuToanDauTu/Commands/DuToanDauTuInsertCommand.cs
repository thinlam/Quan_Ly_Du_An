using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.DuToanDauTus.DTOs;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.DuToanDauTus.Commands;

public record DuToanDauTuInsertCommand(DuToanDauTuDto Dto) : IRequest<DuToanDauTu>;

internal class DuToanDauTuInsertCommandHandler : IRequestHandler<DuToanDauTuInsertCommand, DuToanDauTu>
{
    private readonly IRepository<DuToanDauTu, Guid> _repo;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepo;
    private readonly IUnitOfWork _unitOfWork;

    public DuToanDauTuInsertCommandHandler(IServiceProvider serviceProvider)
    {
        _repo = serviceProvider.GetRequiredService<IRepository<DuToanDauTu, Guid>>();
        _statusRepo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _unitOfWork = _repo.UnitOfWork;
    }

    public async Task<DuToanDauTu> Handle(DuToanDauTuInsertCommand request, CancellationToken cancellationToken = default)
    {
        // Auto-assign Dự thảo status
        var trangThaiDuThao = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == "DT" && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);

        var entity = new DuToanDauTu
        {
            DuAnId = request.Dto.DuAnId,
            BuocId = request.Dto.BuocId,
            SoToTrinh = request.Dto.SoToTrinh,
            NgayTrinh = request.Dto.NgayTrinh,
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
