using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.TrienKhaiKeHoachLCNTMappings;
using QLDA.Application.TrienKhaiKeHoachLCNTs.DTOs;
using QLDA.Domain.Entities;
using QLDA.Domain.Constants;

namespace QLDA.Application.TrienKhaiKeHoachLCNTs.Commands;

public record TrienKhaiKeHoachLCNTUpdateCommand(TrienKhaiKeHoachLCNT Dto) : IRequest<TrienKhaiKeHoachLCNT>;

internal class TrienKhaiKeHoachLCNTUpdateCommandHandler : IRequestHandler<TrienKhaiKeHoachLCNTUpdateCommand, TrienKhaiKeHoachLCNT> {
    private readonly IRepository<TrienKhaiKeHoachLCNT, Guid> _repo;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepo;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationContext _authContext;
    private readonly IUserProvider _user;
    private readonly IUnitOfWork _unitOfWork;

    public TrienKhaiKeHoachLCNTUpdateCommandHandler(IServiceProvider serviceProvider) {
        _repo = serviceProvider.GetRequiredService<IRepository<TrienKhaiKeHoachLCNT, Guid>>();
        _statusRepo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _user = serviceProvider.GetRequiredService<IUserProvider>();
        _unitOfWork = _repo.UnitOfWork;
    }

    public async Task<TrienKhaiKeHoachLCNT> Handle(TrienKhaiKeHoachLCNTUpdateCommand request,
        CancellationToken cancellationToken = default) {
        await _auth.EnsureCanExecuteStepAsync(request.Dto.BuocId, _authContext, cancellationToken);

        var trangThaiDuThao = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DeXuatMacDinh.DuThao && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);
        var trangThaiTraLai = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
        .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DeXuatMacDinh.TraLai && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);

        var entity = await _repo.GetQueryableSet()
            .Include(e => e.DonViTuVans)
            .Include(e => e.TrangThai)
            .FirstOrDefaultAsync(e => e.Id == request.Dto.Id, cancellationToken);
        ManagedException.ThrowIf(entity == null, "Không tìm thấy dữ liệu.");

        if (entity.TrangThaiId != trangThaiDuThao?.Id && entity.TrangThaiId != trangThaiTraLai?.Id)
        {
            throw new ManagedException("Trạng thái không thể cập nhật!");
        }
        entity = request.Dto;
        // entity.SyncGoiThauIds(request.Dto.DanhSachGoiThau);

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await _repo.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return entity;
    }
}
