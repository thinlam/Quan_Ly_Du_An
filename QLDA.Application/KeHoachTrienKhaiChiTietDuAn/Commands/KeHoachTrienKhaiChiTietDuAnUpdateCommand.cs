using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.KeHoachTrienKhaiChiTietDuAnMappings;
using QLDA.Application.KeHoachTrienKhaiChiTietDuAns.DTOs;
using QLDA.Domain.Entities;
using QLDA.Domain.Constants;
using QLDA.Application.Common;
namespace QLDA.Application.KeHoachTrienKhaiChiTietDuAns.Commands;

public record KeHoachTrienKhaiChiTietDuAnUpdateCommand(KeHoachTrienKhaiChiTietDuAn Dto) : IRequest<KeHoachTrienKhaiChiTietDuAn>;

internal class KeHoachTrienKhaiChiTietDuAnUpdateCommandHandler : IRequestHandler<KeHoachTrienKhaiChiTietDuAnUpdateCommand, KeHoachTrienKhaiChiTietDuAn>
{
    private readonly IRepository<KeHoachTrienKhaiChiTietDuAn, Guid> _repo;
    private readonly IUnitOfWork _unitOfWork;

    public KeHoachTrienKhaiChiTietDuAnUpdateCommandHandler(IServiceProvider serviceProvider)
    {
        _repo = serviceProvider.GetRequiredService<IRepository<KeHoachTrienKhaiChiTietDuAn, Guid>>();
        _unitOfWork = _repo.UnitOfWork;
    }

    public async Task<KeHoachTrienKhaiChiTietDuAn> Handle(KeHoachTrienKhaiChiTietDuAnUpdateCommand request,
        CancellationToken cancellationToken = default)
    {
        try
        {

            var entity = await _repo.GetQueryableSet().AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == request.Dto.Id, cancellationToken);
            ManagedException.ThrowIf(entity == null, "Không tìm thấy dữ liệu.");

            await UpdateAsync(request.Dto, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return request.Dto;
        }
        catch (Exception ex)
        {

            throw;
        }
    }
    private async Task UpdateAsync(KeHoachTrienKhaiChiTietDuAn entity, CancellationToken cancellationToken)
    {
        await _repo.UpdateAsync(entity, cancellationToken);
    }
}
