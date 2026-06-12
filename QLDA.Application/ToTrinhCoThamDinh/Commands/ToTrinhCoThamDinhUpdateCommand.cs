using System.Data;
using BuildingBlocks.CrossCutting.ExtensionMethods;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common;
using QLDA.Application.ToTrinhCoThamDinhs.DTOs;
using QLDA.Domain.Constants;

namespace QLDA.Application.ToTrinhCoThamDinhs.Commands;

public record ToTrinhCoThamDinhUpdateCommand(ToTrinhCoThamDinh Dto) : IRequest<ToTrinhCoThamDinh>;

internal class ToTrinhCoThamDinhUpdateCommandHandler : IRequestHandler<ToTrinhCoThamDinhUpdateCommand, ToTrinhCoThamDinh>
{
    private readonly IRepository<ToTrinhCoThamDinh, Guid> _repo;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepo;
    private readonly IUnitOfWork _unitOfWork;

    public ToTrinhCoThamDinhUpdateCommandHandler(IServiceProvider serviceProvider)
    {
        _repo = serviceProvider.GetRequiredService<IRepository<ToTrinhCoThamDinh, Guid>>();
        _statusRepo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _unitOfWork = _repo.UnitOfWork;
    }

    public async Task<ToTrinhCoThamDinh> Handle(ToTrinhCoThamDinhUpdateCommand request, CancellationToken cancellationToken = default)
    {
        var statuses = await _statusRepo.GetByLoaiAsync(PheDuyetEntityNames.QuyetDinhKeHoachThue, cancellationToken);
        var statusDict = statuses.ToDictionary(x => x.Ma);

        var trangThaiDaDuyet = statusDict.GetValueOrDefault(TrangThaiPheDuyetCodes.ToTrinhCoThamDinh.DuThao);
        var trangThaiChoThamDinh = statusDict.GetValueOrDefault(TrangThaiPheDuyetCodes.ToTrinhCoThamDinh.DuThao);


        var entity = await _repo.GetQueryableSet().AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == request.Dto.Id, cancellationToken);
        ManagedException.ThrowIf(entity == null, "Không tìm thấy dữ liệu.");
        // Validate current status must be null (legacy), Dự thảo, or Migrated (LEG)
       
        if (entity.TrangThaiId != trangThaiDaDuyet?.Id && entity.TrangThaiId != trangThaiChoThamDinh?.Id)
        {
            throw new ManagedException("Trạng thái không thể cập nhật!");
        }
        request.Dto.TrangThaiId = entity?.TrangThaiId;

        await _repo.UpdateAsync(request.Dto, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return entity;
    }
}

