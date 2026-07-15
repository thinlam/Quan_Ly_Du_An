using Microsoft.EntityFrameworkCore;
using QLDA.Application.DanhMucTinhTrangThucHienLcnts.DTOs;

namespace QLDA.Application.DanhMucTinhTrangThucHienLcnts.Commands;

public record DanhMucTinhTrangThucHienLcntUpdateCommand(DanhMucTinhTrangThucHienLcntUpdateDto Dto) : IRequest<DanhMucTinhTrangThucHienLcnt>;

internal class DanhMucTinhTrangThucHienLcntUpdateCommandHandler : IRequestHandler<DanhMucTinhTrangThucHienLcntUpdateCommand, DanhMucTinhTrangThucHienLcnt>
{
    private readonly IRepository<DanhMucTinhTrangThucHienLcnt, int> _danhMuc;
    private readonly Serilog.ILogger _logger;

    public DanhMucTinhTrangThucHienLcntUpdateCommandHandler(IServiceProvider serviceProvider)
    {
        _danhMuc = serviceProvider.GetRequiredService<IRepository<DanhMucTinhTrangThucHienLcnt, int>>();
        _logger = Serilog.Log.ForContext<DanhMucTinhTrangThucHienLcntUpdateCommandHandler>();
    }

    public async Task<DanhMucTinhTrangThucHienLcnt> Handle(DanhMucTinhTrangThucHienLcntUpdateCommand request, CancellationToken cancellationToken)
    {
        await ValidateAsync(request, cancellationToken);

        var entity = await _danhMuc.GetOrderedSet()
            .FirstOrDefaultAsync(e => e.Id == request.Dto.Id, cancellationToken: cancellationToken);

        ManagedException.ThrowIf(entity == null, "Danh mục không tồn tại");

        entity!.Ma = request.Dto.Ma;
        entity.Ten = request.Dto.Ten;
        entity.MoTa = request.Dto.MoTa;
        entity.Stt = request.Dto.Stt;
        entity.Used = request.Dto.Used;

        await _danhMuc.AddOrUpdateAsync(entity, cancellationToken);
        await _danhMuc.UnitOfWork.SaveChangesAsync(cancellationToken);

        _logger.Information("Updated {EntityName} with Id {Id}", nameof(DanhMucTinhTrangThucHienLcnt), entity.Id);

        return entity!;
    }

    private async Task ValidateAsync(DanhMucTinhTrangThucHienLcntUpdateCommand request, CancellationToken cancellationToken)
    {
        // Kiểm tra trùng tên (trừ chính nó)
        var exists = await _danhMuc.GetQueryableSet()
            .AnyAsync(e => e.Ten == request.Dto.Ten && e.Id != request.Dto.Id, cancellationToken: cancellationToken);

        ManagedException.ThrowIf(exists, "Tên đã tồn tại");
    }
}
