using QLDA.Application.DanhMucLoaiDieuChinhs.DTOs;

namespace QLDA.Application.DanhMucLoaiDieuChinhs.Commands;

public record DanhMucLoaiDieuChinhInsertCommand(DanhMucLoaiDieuChinhInsertDto Dto) : IRequest<DanhMucLoaiDieuChinhDto>;

internal class DanhMucLoaiDieuChinhInsertCommandHandler : IRequestHandler<DanhMucLoaiDieuChinhInsertCommand, DanhMucLoaiDieuChinhDto> {
    private readonly IRepository<DanhMucLoaiDieuChinh, int> DanhMucLoaiDieuChinh;
    private readonly IUnitOfWork UnitOfWork;

    public DanhMucLoaiDieuChinhInsertCommandHandler(IServiceProvider serviceProvider) {
        DanhMucLoaiDieuChinh = serviceProvider.GetRequiredService<IRepository<DanhMucLoaiDieuChinh, int>>();
        UnitOfWork = DanhMucLoaiDieuChinh.UnitOfWork;
    }

    public async Task<DanhMucLoaiDieuChinhDto> Handle(DanhMucLoaiDieuChinhInsertCommand request, CancellationToken cancellationToken) {
        var entity = new DanhMucLoaiDieuChinh {
            Ma = request.Dto.Ma,
            Ten = request.Dto.Ten,
            MoTa = request.Dto.MoTa,
            Stt = request.Dto.Stt,
            Used = request.Dto.Used,
        };

        await DanhMucLoaiDieuChinh.AddAsync(entity, cancellationToken);
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