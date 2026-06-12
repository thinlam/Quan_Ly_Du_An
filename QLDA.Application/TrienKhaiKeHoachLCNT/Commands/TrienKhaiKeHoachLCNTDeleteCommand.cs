using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common;
using QLDA.Domain.Constants;

namespace QLDA.Application.TrienKhaiKeHoachLCNTs.Commands;

public record TrienKhaiKeHoachLCNTDeleteCommand(Guid Id) : IRequest<int>
{
}

public record TrienKhaiKeHoachLCNTDeleteCommandHandler : IRequestHandler<TrienKhaiKeHoachLCNTDeleteCommand, int>
{
    private readonly IRepository<TrienKhaiKeHoachLCNT, Guid> TrienKhaiKeHoachLCNT;
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository;

    public TrienKhaiKeHoachLCNTDeleteCommandHandler(IServiceProvider serviceProvider)
    {
        TrienKhaiKeHoachLCNT = serviceProvider.GetRequiredService<IRepository<TrienKhaiKeHoachLCNT, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
        _statusRepository = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _unitOfWork = TrienKhaiKeHoachLCNT.UnitOfWork;
    }

    public async Task<int> Handle(TrienKhaiKeHoachLCNTDeleteCommand request, CancellationToken cancellationToken)
    {
        var entity = await TrienKhaiKeHoachLCNT.GetOrderedSet()
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity);
        var trangThaiDuThao = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
          .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DeXuatMacDinh.DuThao && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);

        if (entity.TrangThaiId != null && entity.TrangThaiId != trangThaiDuThao?.Id)
        {
            throw new ManagedException("Tờ trình đang ở trạng thái không thể xóa!");
        }

        entity.IsDeleted = true;

        await SyncHelper.SetDeleteWithRelatedFiles(TepDinhKem, [entity.Id.ToString()], cancellationToken);

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}