using System.Data;
using System.Drawing;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common;
using QLDA.Application.KeHoachTrienKhaiChiTietDuAnMappings;
using QLDA.Application.KeHoachTrienKhaiChiTietDuAns.DTOs;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities;
namespace QLDA.Application.KeHoachTrienKhaiChiTietDuAns.Commands;

public record KeHoachTrienKhaiChiTietDuAnUpdateCommand(KeHoachTrienKhaiChiTietDuAn model) : IRequest<KeHoachTrienKhaiChiTietDuAn>;

internal class KeHoachTrienKhaiChiTietDuAnUpdateCommandHandler : IRequestHandler<KeHoachTrienKhaiChiTietDuAnUpdateCommand, KeHoachTrienKhaiChiTietDuAn>
{
    private readonly IRepository<KeHoachTrienKhaiChiTietDuAn, Guid> _repo;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationManager _authManager;
    private readonly IAuthorizationContext _authContext;
    private readonly IUnitOfWork _unitOfWork;

    public KeHoachTrienKhaiChiTietDuAnUpdateCommandHandler(IServiceProvider serviceProvider)
    {
        _repo = serviceProvider.GetRequiredService<IRepository<KeHoachTrienKhaiChiTietDuAn, Guid>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authManager = serviceProvider.GetRequiredService<IAuthorizationManager>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _unitOfWork = _repo.UnitOfWork;
    }

    public async Task<KeHoachTrienKhaiChiTietDuAn> Handle(KeHoachTrienKhaiChiTietDuAnUpdateCommand request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _repo.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.model.Id, cancellationToken);
        ManagedException.ThrowIf(entity == null, "Không tìm thấy dữ liệu.");
        var model = request.model;
        // Check step authorization
        await _auth.EnsureCanExecuteStepAsync(request.model.BuocId, _authContext, cancellationToken);
        await _authManager.EnsureCanExecuteAsync(request.model.BuocId, request.model.DuAnId, _authContext, cancellationToken);
        entity.BuocId = model.BuocId;
        entity.DuAnId = model.DuAnId;

        entity.GhiChu = model.GhiChu;
        entity.MaMoc = model.MaMoc;
        entity.Ten = model.Ten;
        entity.NgayBatDauKeHoach = model.NgayBatDauKeHoach;
        entity.NgayBatDauThucTe = model.NgayBatDauThucTe;
        entity.NgayKetThucKeHoach = model.NgayKetThucKeHoach;
        entity.NgayKetThucThucTe = model.NgayKetThucThucTe;
        entity.TiLeHoanThanh = model.TiLeHoanThanh;
        entity.TrangThaiId = model.TrangThaiId;
        entity.DonViChuTriId = model.DonViChuTriId;
        await UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return entity;
    }
    private async Task UpdateAsync(KeHoachTrienKhaiChiTietDuAn entity, CancellationToken cancellationToken)
    {
        await _repo.UpdateAsync(entity, cancellationToken);
    }
}
