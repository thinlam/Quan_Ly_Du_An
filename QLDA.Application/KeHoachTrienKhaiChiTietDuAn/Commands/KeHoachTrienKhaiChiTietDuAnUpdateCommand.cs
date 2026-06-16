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
    private readonly IRepository<DuAnBuoc, int> _duAnBuocRepo;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IUserProvider _user;
    private readonly IUnitOfWork _unitOfWork;

    public KeHoachTrienKhaiChiTietDuAnUpdateCommandHandler(IServiceProvider serviceProvider)
    {
        _repo = serviceProvider.GetRequiredService<IRepository<KeHoachTrienKhaiChiTietDuAn, Guid>>();
        _duAnBuocRepo = serviceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _user = serviceProvider.GetRequiredService<IUserProvider>();
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

            if (request.Dto.BuocId.HasValue)
            {
                var buoc = await _duAnBuocRepo.GetQueryableSet()
                    .Include(e => e.DuAn)
                    .Include(e => e.DuAnBuocPhongBanPhoiHops)
                    .FirstOrDefaultAsync(e => e.Id == request.Dto.BuocId.Value, cancellationToken);
                if (buoc != null && !await _auth.CanExecuteStepAsync(buoc, _user, cancellationToken))
                    throw new ManagedException("Phòng ban không có quyền thao tác bước này");
            }

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
