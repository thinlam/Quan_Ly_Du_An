using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common;
using QLDA.Domain.Constants;

namespace QLDA.Application.KeHoachLuaChonNhaThauRutGons.Commands;

public record KeHoachLuaChonNhaThauRutGonDeleteCommand(Guid Id) : IRequest<int>
{
}

public record KeHoachLuaChonNhaThauRutGonDeleteCommandHandler : IRequestHandler<KeHoachLuaChonNhaThauRutGonDeleteCommand, int>
{
    private readonly IRepository<KeHoachLuaChonNhaThauRutGon, Guid> KeHoachLuaChonNhaThauRutGon;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository;
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem;
    private readonly IUnitOfWork _unitOfWork;

    public KeHoachLuaChonNhaThauRutGonDeleteCommandHandler(IServiceProvider serviceProvider)
    {
        KeHoachLuaChonNhaThauRutGon = serviceProvider.GetRequiredService<IRepository<KeHoachLuaChonNhaThauRutGon, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
        _statusRepository = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _unitOfWork = KeHoachLuaChonNhaThauRutGon.UnitOfWork;
    }

    public async Task<int> Handle(KeHoachLuaChonNhaThauRutGonDeleteCommand request, CancellationToken cancellationToken)
    {
        var entity = await KeHoachLuaChonNhaThauRutGon.GetOrderedSet()
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);
        ManagedException.ThrowIfNull(entity);
        
        var trangThaiDuThao = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.KeHoachLuaChonNhaThauRutGon.DuThao && s.Loai == PheDuyetEntityNames.KeHoachLuaChonNhaThauRutGon, cancellationToken);

        if (entity.TrangThaiId != null && entity.TrangThaiId != trangThaiDuThao?.Id)
        {
            throw new ManagedException("Tờ trình đang ở trạng thái không thể xóa!");
        }

        entity.IsDeleted = true;

        await SyncHelper.SetDeleteWithRelatedFiles(TepDinhKem, [entity.Id.ToString()], cancellationToken);

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}