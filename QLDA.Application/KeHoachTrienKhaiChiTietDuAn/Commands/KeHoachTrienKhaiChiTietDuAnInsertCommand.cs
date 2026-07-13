using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.KeHoachTrienKhaiChiTietDuAnMappings;
using QLDA.Application.KeHoachTrienKhaiChiTietDuAns.DTOs;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.KeHoachTrienKhaiChiTietDuAns.Commands;

public record KeHoachTrienKhaiChiTietDuAnInsertCommand(KeHoachTrienKhaiChiTietDuAn entity) : IRequest<KeHoachTrienKhaiChiTietDuAn>;

internal class KeHoachTrienKhaiChiTietDuAnInsertCommandHandler : IRequestHandler<KeHoachTrienKhaiChiTietDuAnInsertCommand, KeHoachTrienKhaiChiTietDuAn> {
    private readonly IRepository<KeHoachTrienKhaiChiTietDuAn, Guid> _repo;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationManager _authManager;
    private readonly IAuthorizationContext _authContext;
    private readonly IUnitOfWork _unitOfWork;

    public KeHoachTrienKhaiChiTietDuAnInsertCommandHandler(IServiceProvider serviceProvider) {
        _repo = serviceProvider.GetRequiredService<IRepository<KeHoachTrienKhaiChiTietDuAn, Guid>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authManager = serviceProvider.GetRequiredService<IAuthorizationManager>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _unitOfWork = _repo.UnitOfWork;
    }

    public async Task<KeHoachTrienKhaiChiTietDuAn> Handle(KeHoachTrienKhaiChiTietDuAnInsertCommand request,
        CancellationToken cancellationToken = default) {

        // Check step authorization
        await _auth.EnsureCanExecuteStepAsync(request.entity.BuocId, _authContext, cancellationToken);
        await _authManager.EnsureCanExecuteAsync(request.entity.BuocId, request.entity.DuAnId, _authContext, cancellationToken);

        await _repo.AddAsync(request.entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return request.entity;
    }
}
