using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common;
using QLDA.Application.ToTrinhCoThamDinhs.DTOs;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities.DanhMuc;
using Serilog;

namespace QLDA.Application.ToTrinhCoThamDinhs.Commands;

public record ToTrinhCoThamDinhInsertCommand(ToTrinhCoThamDinh Dto) : IRequest<ToTrinhCoThamDinh>;

internal class ToTrinhCoThamDinhInsertCommandHandler : IRequestHandler<ToTrinhCoThamDinhInsertCommand, ToTrinhCoThamDinh>
{
    private readonly IRepository<ToTrinhCoThamDinh, Guid> _repo;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepo;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationContext _authContext;
    private readonly IUnitOfWork _unitOfWork;

    public ToTrinhCoThamDinhInsertCommandHandler(IServiceProvider serviceProvider)
    {
        _repo = serviceProvider.GetRequiredService<IRepository<ToTrinhCoThamDinh, Guid>>();
        _statusRepo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _unitOfWork = _repo.UnitOfWork;
    }

    public async Task<ToTrinhCoThamDinh> Handle(ToTrinhCoThamDinhInsertCommand request, CancellationToken cancellationToken = default)
    {
        await _auth.EnsureCanExecuteStepAsync(request.Dto.BuocId, _authContext, cancellationToken);

        // Auto-assign Dự thảo status

        var trangThaiDuThao = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.ToTrinhCoThamDinh.DuThao
            && s.Loai == PheDuyetEntityNames.ToTrinhCoThamDinh, cancellationToken);
            ManagedException.ThrowIf(trangThaiDuThao == null, "Không tìm thấy trạng thái cần cập nhật!");
        try
        {
            var entity = request.Dto;
            entity.TrangThaiId = trangThaiDuThao?.Id;
            request.Dto.TrangThaiId = trangThaiDuThao?.Id;

            await _repo.AddAsync(request.Dto, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return entity;
        }
        catch (Exception ex)
        {
            Log.Information($"ToTrinhCoThamDinhInsertCommand error {ex.Message}");
            throw;
        }


    }
}
