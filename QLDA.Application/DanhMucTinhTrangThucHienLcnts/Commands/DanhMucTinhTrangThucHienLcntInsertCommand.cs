using Microsoft.EntityFrameworkCore;
using QLDA.Application.DanhMucTinhTrangThucHienLcnts.DTOs;

namespace QLDA.Application.DanhMucTinhTrangThucHienLcnts.Commands;

public record DanhMucTinhTrangThucHienLcntInsertCommand(DanhMucTinhTrangThucHienLcntInsertDto Dto) : IRequest<DanhMucTinhTrangThucHienLcnt>;

internal class DanhMucTinhTrangThucHienLcntInsertCommandHandler : IRequestHandler<DanhMucTinhTrangThucHienLcntInsertCommand, DanhMucTinhTrangThucHienLcnt>
{
    private readonly IRepository<DanhMucTinhTrangThucHienLcnt, int> _danhMuc;
    private readonly Serilog.ILogger _logger;

    public DanhMucTinhTrangThucHienLcntInsertCommandHandler(IServiceProvider serviceProvider)
    {
        _danhMuc = serviceProvider.GetRequiredService<IRepository<DanhMucTinhTrangThucHienLcnt, int>>();
        _logger = Serilog.Log.ForContext<DanhMucTinhTrangThucHienLcntInsertCommandHandler>();
    }

    public async Task<DanhMucTinhTrangThucHienLcnt> Handle(DanhMucTinhTrangThucHienLcntInsertCommand request, CancellationToken cancellationToken)
    {
        await ValidateAsync(request, cancellationToken);

        var entity = new DanhMucTinhTrangThucHienLcnt
        {
            Ma = request.Dto.Ma,
            Ten = request.Dto.Ten,
            MoTa = request.Dto.MoTa,
            Stt = request.Dto.Stt,
            Used = request.Dto.Used
        };

        await _danhMuc.AddAsync(entity, cancellationToken);
        await _danhMuc.UnitOfWork.SaveChangesAsync(cancellationToken);

        _logger.Information("Created {EntityName} with Id {Id}", nameof(DanhMucTinhTrangThucHienLcnt), entity.Id);

        return entity!;
    }

    private async Task ValidateAsync(DanhMucTinhTrangThucHienLcntInsertCommand request, CancellationToken cancellationToken)
    {
        // Kiểm tra trùng tên
        var exists = await _danhMuc.GetQueryableSet()
            .AnyAsync(e => e.Ten == request.Dto.Ten, cancellationToken: cancellationToken);

        ManagedException.ThrowIf(exists, "Tên đã tồn tại");
    }
}
