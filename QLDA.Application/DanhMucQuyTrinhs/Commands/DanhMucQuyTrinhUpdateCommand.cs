using Microsoft.EntityFrameworkCore;
using QLDA.Application.DanhMucQuyTrinhs.DTOs;

namespace QLDA.Application.DanhMucQuyTrinhs.Commands;

public record DanhMucQuyTrinhUpdateCommand(DanhMucQuyTrinhDto Dto) : IRequest<DanhMucQuyTrinhDto>;

internal class DanhMucQuyTrinhUpdateCommandHandler : IRequestHandler<DanhMucQuyTrinhUpdateCommand, DanhMucQuyTrinhDto> {
    private readonly IRepository<DanhMucQuyTrinh, int> DanhMucQuyTrinh;
    private readonly IUnitOfWork UnitOfWork;

    public DanhMucQuyTrinhUpdateCommandHandler(IServiceProvider serviceProvider) {
        DanhMucQuyTrinh = serviceProvider.GetRequiredService<IRepository<DanhMucQuyTrinh, int>>();
        UnitOfWork = DanhMucQuyTrinh.UnitOfWork;
    }

    public async Task<DanhMucQuyTrinhDto> Handle(DanhMucQuyTrinhUpdateCommand request, CancellationToken cancellationToken) {
        var entity = await DanhMucQuyTrinh.GetQueryableSet(OnlyUsed: false).FirstOrDefaultAsync(e => e.Id == request.Dto.Id, cancellationToken);
        ManagedException.ThrowIf(entity == null, "Danh mục quy trình không tồn tại.");

        entity!.Ma = request.Dto.Ma;
        entity.Ten = request.Dto.Ten;
        entity.MoTa = request.Dto.MoTa;
        entity.Stt = request.Dto.Stt;
        entity.Used = request.Dto.Used;
        entity.MacDinh = request.Dto.MacDinh;

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToDto();
    }
}