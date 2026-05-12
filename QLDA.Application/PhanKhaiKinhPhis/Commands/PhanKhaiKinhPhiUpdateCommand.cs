using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.PhanKhaiKinhPhis.DTOs;

namespace QLDA.Application.PhanKhaiKinhPhis.Commands;

public record PhanKhaiKinhPhiUpdateCommand(PhanKhaiKinhPhiUpdateDto Dto) : IRequest<PhanKhaiKinhPhi>;

internal class PhanKhaiKinhPhiUpdateCommandHandler : IRequestHandler<PhanKhaiKinhPhiUpdateCommand, PhanKhaiKinhPhi> {
    private readonly IRepository<PhanKhaiKinhPhi, Guid> _repo;
    private readonly IUnitOfWork _unitOfWork;

    public PhanKhaiKinhPhiUpdateCommandHandler(IServiceProvider serviceProvider) {
        _repo = serviceProvider.GetRequiredService<IRepository<PhanKhaiKinhPhi, Guid>>();
        _unitOfWork = _repo.UnitOfWork;
    }

    public async Task<PhanKhaiKinhPhi> Handle(PhanKhaiKinhPhiUpdateCommand request, CancellationToken cancellationToken = default) {
        var entity = await _repo.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Dto.Id, cancellationToken);
        ManagedException.ThrowIf(entity == null, "Không tìm thấy phân khai kinh phí");

        entity.SoToTrinh = request.Dto.SoToTrinh;
        entity.NgayToTrinh = request.Dto.NgayToTrinh;
        entity.NguonVonId = request.Dto.NguonVonId;
        entity.KinhPhiDeXuat = request.Dto.KinhPhiDeXuat;
        entity.KinhPhiPhanKhai = request.Dto.KinhPhiPhanKhai;
        entity.ThuyetMinh = request.Dto.ThuyetMinh;
        entity.DuAnId = request.Dto.DuAnId;

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await _repo.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return entity;
    }
}
