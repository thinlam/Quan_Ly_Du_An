using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.BaoCaoKetQuaKhaoSats.DTOs;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.BaoCaoKetQuaKhaoSats.Commands;

public record BaoCaoKetQuaKhaoSatUpdateCommand(BaoCaoKetQuaKhaoSatUpdateModel Model)
    : IRequest<BaoCaoKetQuaKhaoSat>;

internal class BaoCaoKetQuaKhaoSatUpdateCommandHandler
    : IRequestHandler<BaoCaoKetQuaKhaoSatUpdateCommand, BaoCaoKetQuaKhaoSat>
{
    private readonly IRepository<BaoCaoKetQuaKhaoSat, Guid> _repository;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepo;
    private readonly IUnitOfWork _unitOfWork;

    public BaoCaoKetQuaKhaoSatUpdateCommandHandler(IServiceProvider serviceProvider)
    {
        _repository = serviceProvider.GetRequiredService<IRepository<BaoCaoKetQuaKhaoSat, Guid>>();
        _statusRepo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task<BaoCaoKetQuaKhaoSat> Handle(
        BaoCaoKetQuaKhaoSatUpdateCommand request,
        CancellationToken cancellationToken = default)
    {
        var trangThaiDuThao = await _statusRepo
            .GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s =>
                s.Ma == TrangThaiPheDuyetCodes.BaoCaoKetQuaKhaoSat.DuThao &&
                s.Loai == PheDuyetEntityNames.BaoCaoKetQuaKhaoSat, cancellationToken);
        var trangThaiTraLai = await _statusRepo
            .GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s =>
                s.Ma == TrangThaiPheDuyetCodes.BaoCaoKetQuaKhaoSat.TraLai &&
                s.Loai == PheDuyetEntityNames.BaoCaoKetQuaKhaoSat, cancellationToken);

        var entity = await _repository.GetQueryableSet()
            .Include(e => e.TrangThai)
            .FirstOrDefaultAsync(e => e.Id == request.Model.Id, cancellationToken);
        ManagedException.ThrowIfNull(entity, "Không tìm thấy báo cáo kết quả khảo sát");

        if (entity.TrangThaiId != null &&
            entity.TrangThaiId != trangThaiDuThao?.Id &&
            entity.TrangThaiId != trangThaiTraLai?.Id &&
            entity.TrangThai?.Ma != "LEG")
        {
            throw new ManagedException("Chỉ có thể cập nhật khi trạng thái là Dự thảo hoặc Trả lại");
        }

        entity.Update(request.Model);

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await _repository.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return entity;
    }
}
