using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common;
using QLDA.Application.ToTrinhPheDuyets.DTOs;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.ToTrinhPheDuyets.Commands;

public record ToTrinhPheDuyetInsertCommand(ToTrinhPheDuyetInsUpdDto Dto) : IRequest<ToTrinhPheDuyet>;

internal class ToTrinhPheDuyetInsertCommandHandler : IRequestHandler<ToTrinhPheDuyetInsertCommand, ToTrinhPheDuyet>
{
    private readonly IRepository<ToTrinhPheDuyet, Guid> _repo;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepo;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationContext _authContext;
    private readonly IUserProvider _user;
    private readonly IUnitOfWork _unitOfWork;

    public ToTrinhPheDuyetInsertCommandHandler(IServiceProvider serviceProvider)
    {
        _repo = serviceProvider.GetRequiredService<IRepository<ToTrinhPheDuyet, Guid>>();
        _statusRepo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _user = serviceProvider.GetRequiredService<IUserProvider>();
        _unitOfWork = _repo.UnitOfWork;
    }

    public async Task<ToTrinhPheDuyet> Handle(ToTrinhPheDuyetInsertCommand request, CancellationToken cancellationToken = default)
    {
        await _auth.EnsureCanExecuteStepAsync(request.Dto.BuocId, _authContext, cancellationToken);

        // Auto-assign Dự thảo status
        // hiện tại tờ trình có 2 loại trạng thái là ToTrinhKhongDuyet & DeXuatMacDinh -> lấy trạng thái đúng loại
        bool isKhongDuyet = LoaiToTrinhKhongDuyetExtensions.ContainsDescription(request.Dto.Loai);

        var loaiPheDuyet = isKhongDuyet ? PheDuyetEntityNames.ToTrinhKhongDuyet : PheDuyetEntityNames.DeXuatMacDinhStt;
        var statuses = await _statusRepo.GetByLoaiAsync(loaiPheDuyet, cancellationToken);
        var statusDict = statuses
            .Where(x => !string.IsNullOrWhiteSpace(x.Ma))
            .ToDictionary(x => x.Ma!, x => x);
        var trangThaiDuThao = statusDict.GetValueOrDefault(TrangThaiPheDuyetCodes.DeXuatMacDinh.DuThao);


        var entity = new ToTrinhPheDuyet
        {
            DuAnId = request.Dto.DuAnId,
            BuocId = request.Dto.BuocId,
            So = request.Dto.So,
            NgayToTrinh = request.Dto.NgayToTrinh,
            TrichYeu = request.Dto.TrichYeu,
            Loai = request.Dto.Loai,
            TrangThaiId = trangThaiDuThao?.Id,
        };

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await _repo.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return entity;
    }
}
