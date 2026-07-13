using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common;
using QLDA.Application.ToTrinhCoThamDinhs.DTOs;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities.DanhMuc;
using Serilog;

namespace QLDA.Application.ToTrinhCoThamDinhs.Commands;

public record ToTrinhCoThamDinhInsertCommand(ToTrinhCoThamDinhInsUpdDto Dto) : IRequest<ToTrinhCoThamDinh>;

internal class ToTrinhCoThamDinhInsertCommandHandler : IRequestHandler<ToTrinhCoThamDinhInsertCommand, ToTrinhCoThamDinh>
{
    private readonly IRepository<ToTrinhCoThamDinh, Guid> _repo;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepo;
    private readonly IAuthorizationManager _authManager;
    private readonly IAuthorizationContext _authContext;
    private readonly IUnitOfWork _unitOfWork;

    public ToTrinhCoThamDinhInsertCommandHandler(IServiceProvider serviceProvider)
    {
        _repo = serviceProvider.GetRequiredService<IRepository<ToTrinhCoThamDinh, Guid>>();
        _statusRepo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _authManager = serviceProvider.GetRequiredService<IAuthorizationManager>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _unitOfWork = _repo.UnitOfWork;
    }

    public async Task<ToTrinhCoThamDinh> Handle(ToTrinhCoThamDinhInsertCommand request, CancellationToken cancellationToken = default)
    {
        await _authManager.EnsureCanExecuteAsync(request.Dto.BuocId, request.Dto.DuAnId, _authContext, cancellationToken);

        // Auto-assign Dự thảo status

        var trangThaiDuThao = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.ToTrinhCoThamDinh.DuThao
            && s.Loai == PheDuyetEntityNames.ToTrinhCoThamDinh, cancellationToken);
            ManagedException.ThrowIf(trangThaiDuThao == null, "Không tìm thấy trạng thái cần cập nhật!");
        try
        {
            ToTrinhCoThamDinh entity = new ToTrinhCoThamDinh() {
                Id = request.Dto.Id is null || request.Dto.Id == Guid.Empty? Guid.NewGuid(): request.Dto.Id.Value
            };
             request.Dto.MapToEntity(entity);
            entity.TrangThaiId = trangThaiDuThao?.Id;
            await _repo.AddAsync(entity, cancellationToken);
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
