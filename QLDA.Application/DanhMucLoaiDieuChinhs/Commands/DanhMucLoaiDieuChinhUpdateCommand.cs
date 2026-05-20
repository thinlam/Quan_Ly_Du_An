using Microsoft.EntityFrameworkCore;
using QLDA.Application.DanhMucLoaiDieuChinhs.DTOs;

namespace QLDA.Application.DanhMucLoaiDieuChinhs.Commands;

public record DanhMucLoaiDieuChinhUpdateCommand(DanhMucLoaiDieuChinhUpdateDto Dto) : IRequest<DanhMucLoaiDieuChinhDto>;

internal class DanhMucLoaiDieuChinhUpdateCommandHandler : IRequestHandler<DanhMucLoaiDieuChinhUpdateCommand, DanhMucLoaiDieuChinhDto> {
    private readonly IRepository<DanhMucLoaiDieuChinh, int> DanhMucLoaiDieuChinh;
    private readonly IUnitOfWork UnitOfWork;

    public DanhMucLoaiDieuChinhUpdateCommandHandler(IServiceProvider serviceProvider) {
        DanhMucLoaiDieuChinh = serviceProvider.GetRequiredService<IRepository<DanhMucLoaiDieuChinh, int>>();
        UnitOfWork = DanhMucLoaiDieuChinh.UnitOfWork;
    }

    public async Task<DanhMucLoaiDieuChinhDto> Handle(DanhMucLoaiDieuChinhUpdateCommand request, CancellationToken cancellationToken) {
        var entity = await DanhMucLoaiDieuChinh.GetOrderedSet().FirstOrDefaultAsync(e => e.Id == request.Dto.Id, cancellationToken);
        ManagedException.ThrowIf(entity == null, $"Không tìm thấy loại điều chỉnh có ID {request.Dto.Id}");

        entity.Ma = request.Dto.Ma;
        entity.Ten = request.Dto.Ten;
        entity.MoTa = request.Dto.MoTa;
        entity.Stt = request.Dto.Stt;
        entity.Used = request.Dto.Used;

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return new DanhMucLoaiDieuChinhDto {
            Id = entity.Id,
            Ma = entity.Ma,
            Ten = entity.Ten,
            MoTa = entity.MoTa,
            Stt = entity.Stt,
            Used = entity.Used,
        };
    }
}