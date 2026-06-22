using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
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
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationContext _authContext;
    private readonly IUnitOfWork _unitOfWork;

    public KeHoachTrienKhaiChiTietDuAnUpdateCommandHandler(IServiceProvider serviceProvider)
    {
        _repo = serviceProvider.GetRequiredService<IRepository<KeHoachTrienKhaiChiTietDuAn, Guid>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _unitOfWork = _repo.UnitOfWork;
    }

    public async Task<KeHoachTrienKhaiChiTietDuAn> Handle(KeHoachTrienKhaiChiTietDuAnUpdateCommand request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _repo.GetQueryableSet().AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == request.Dto.Id, cancellationToken);
        ManagedException.ThrowIf(entity == null, "Không tìm thấy dữ liệu.");

        // Check step authorization
        await _auth.EnsureCanExecuteStepAsync(request.Dto.BuocId, _authContext, cancellationToken);

        await UpdateAsync(request.Dto, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return request.Dto;
    }
    private async Task UpdateAsync(KeHoachTrienKhaiChiTietDuAn entity, CancellationToken cancellationToken)
    {
        await _repo.UpdateAsync(entity, cancellationToken);
    }
}
