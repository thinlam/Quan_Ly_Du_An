using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.KeHoachTrienKhaiChiTietDuAnMappings;
using QLDA.Application.KeHoachTrienKhaiChiTietDuAns.DTOs;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.KeHoachTrienKhaiChiTietDuAns.Commands;

public record KeHoachTrienKhaiChiTietDuAnInsertCommand(KeHoachTrienKhaiChiTietDuAn entity) : IRequest<KeHoachTrienKhaiChiTietDuAn>;

internal class KeHoachTrienKhaiChiTietDuAnInsertCommandHandler : IRequestHandler<KeHoachTrienKhaiChiTietDuAnInsertCommand, KeHoachTrienKhaiChiTietDuAn> {
    private readonly IRepository<KeHoachTrienKhaiChiTietDuAn, Guid> _repo;
    private readonly IUnitOfWork _unitOfWork;

    public KeHoachTrienKhaiChiTietDuAnInsertCommandHandler(IServiceProvider serviceProvider) {
        _repo = serviceProvider.GetRequiredService<IRepository<KeHoachTrienKhaiChiTietDuAn, Guid>>();
        _unitOfWork = _repo.UnitOfWork;
    }

    public async Task<KeHoachTrienKhaiChiTietDuAn> Handle(KeHoachTrienKhaiChiTietDuAnInsertCommand request,
        CancellationToken cancellationToken = default) {
       
        await _repo.AddAsync(request.entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return request.entity;
    }
}
