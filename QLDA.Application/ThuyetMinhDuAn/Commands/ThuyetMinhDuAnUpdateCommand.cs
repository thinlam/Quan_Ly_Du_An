using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Providers;
using QLDA.Application.ThuyetMinhDuAns.DTOs;
using QLDA.Domain.Constants;
using System.Data;

namespace QLDA.Application.ThuyetMinhDuAns.Commands;

public record ThuyetMinhDuAnUpdateCommand(ThuyetMinhDuAn Dto) : IRequest<ThuyetMinhDuAn>;

internal class ThuyetMinhDuAnUpdateCommandHandler : IRequestHandler<ThuyetMinhDuAnUpdateCommand, ThuyetMinhDuAn>
{
    private readonly IRepository<ThuyetMinhDuAn, Guid> _repo;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepo;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationContext _authContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserProvider _userProvider;
    readonly IAppSettingsProvider _settings;

    public ThuyetMinhDuAnUpdateCommandHandler(IServiceProvider serviceProvider)
    {
        _repo = serviceProvider.GetRequiredService<IRepository<ThuyetMinhDuAn, Guid>>();
        _statusRepo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _settings = serviceProvider.GetRequiredService<IAppSettingsProvider>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _unitOfWork = _repo.UnitOfWork;
    }

    public async Task<ThuyetMinhDuAn> Handle(ThuyetMinhDuAnUpdateCommand request, CancellationToken cancellationToken = default)
    {
        var isHcth = _userProvider.Info.PhongBanID == _settings.PhongHCTHId;
        var trangThaiDuThao = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DeXuatMacDinh.DuThao && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);
        var traLaiStt = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
           .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DeXuatMacDinh.TraLai && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);
        var daDuyetStt = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
           .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DeXuatMacDinh.DaDuyet && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);

        var entity = await _repo.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Dto.Id, cancellationToken);
        ManagedException.ThrowIf(entity == null, "Không tìm thấy dữ liệu.");

        await _auth.EnsureCanExecuteStepAsync(entity.BuocId, _authContext, cancellationToken);

        // Validate current status must be null (legacy), Dự thảo, or Migrated (LEG)
        //if (entity.TrangThaiId != null && entity.TrangThaiId != trangThaiDuThao?.Id && entity.TrangThaiId != traLaiStt?.Id)
        //{
        //    throw new ManagedException("Chỉ có thể cập nhật khi trạng thái là dự thảo hoặc trả lại");
        // }

        if (entity.TrangThaiId != null && (entity.TrangThaiId == trangThaiDuThao?.Id || entity.TrangThaiId == traLaiStt?.Id))
        {
            entity.So = request.Dto.So;
            entity.NgayTrinh = request.Dto.NgayTrinh;
            entity.TrichYeu = request.Dto.TrichYeu;
            entity.DuAnId = request.Dto.DuAnId;
            entity.BuocId = request.Dto.BuocId;

        }
        else if((entity.TrangThaiId == daDuyetStt?.Id || entity.TrangThaiId == traLaiStt?.Id) && isHcth)
        {
            entity.TrangThaiThamDinhId = request.Dto.TrangThaiThamDinhId;
            entity.KetQuaThamDinh = request.Dto.KetQuaThamDinh;
        }
        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await _repo.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return entity;
    }
}

