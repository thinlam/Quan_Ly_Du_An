using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.TrienKhaiKeHoachLCNTMappings;
using QLDA.Application.TrienKhaiKeHoachLCNTs.DTOs;
using QLDA.Domain.Entities;
using QLDA.Domain.Constants;

namespace QLDA.Application.TrienKhaiKeHoachLCNTs.Commands;

public record TrienKhaiKeHoachLCNTUpdateCommand(TrienKhaiKeHoachLCNT Dto) : IRequest<TrienKhaiKeHoachLCNT>;

internal class TrienKhaiKeHoachLCNTUpdateCommandHandler : IRequestHandler<TrienKhaiKeHoachLCNTUpdateCommand, TrienKhaiKeHoachLCNT>
{
    private readonly IRepository<TrienKhaiKeHoachLCNT, Guid> _repo;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepo;
    private readonly IAuthorizationManager _authManager;
    private readonly IAuthorizationContext _authContext;
    private readonly IUserProvider _user;
    private readonly IUnitOfWork _unitOfWork;

    public TrienKhaiKeHoachLCNTUpdateCommandHandler(IServiceProvider serviceProvider)
    {
        _repo = serviceProvider.GetRequiredService<IRepository<TrienKhaiKeHoachLCNT, Guid>>();
        _statusRepo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _authManager = serviceProvider.GetRequiredService<IAuthorizationManager>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _user = serviceProvider.GetRequiredService<IUserProvider>();
        _unitOfWork = _repo.UnitOfWork;
    }

    public async Task<TrienKhaiKeHoachLCNT> Handle(TrienKhaiKeHoachLCNTUpdateCommand request,
        CancellationToken cancellationToken = default)
    {
        await _authManager.EnsureCanExecuteAsync(request.Dto.BuocId, request.Dto.DuAnId, _authContext, cancellationToken);

        var trangThaiDuThao = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DeXuatMacDinh.DuThao && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);
        var trangThaiTraLai = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
        .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DeXuatMacDinh.TraLai && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);

        var entity = await _repo.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Dto.Id, cancellationToken);
        ManagedException.ThrowIf(entity == null, "Không tìm thấy dữ liệu.");

        if (entity.TrangThaiId != trangThaiDuThao?.Id && entity.TrangThaiId != trangThaiTraLai?.Id)
        {
            throw new ManagedException("Trạng thái không thể cập nhật!");
        }
        entity.BuocId = request.Dto.BuocId;
        entity.DuAnId = request.Dto.DuAnId;
        entity.NgayTrinh = request.Dto.NgayTrinh;
        entity.TrichYeu = request.Dto.TrichYeu;
        entity.So = request.Dto.So;

        entity.NoiDung = request.Dto.NoiDung;
        entity.YeuCau = request.Dto.YeuCau;
        entity.GiaTri = request.Dto.GiaTri;
        entity.GoiThauId = request.Dto.GoiThauId;
        entity.HinhThucLCNT = request.Dto.HinhThucLCNT;
        entity.TrangThaiDangTaiId = request.Dto.TrangThaiDangTaiId;
        entity.ThoiGianThucHien = request.Dto.ThoiGianThucHien;

        var dbContext = _unitOfWork as DbContext
            ?? throw new InvalidOperationException("UnitOfWork must be a DbContext.");
        await dbContext.SyncDonViTuVanAsync(entity.Id, request.Dto.DonViTuVans, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return entity;
    }
}
