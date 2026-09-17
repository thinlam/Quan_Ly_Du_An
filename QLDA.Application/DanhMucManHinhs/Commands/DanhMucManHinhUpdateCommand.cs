using Microsoft.EntityFrameworkCore;
using QLDA.Application.DanhMucManHinhs.DTOs;

namespace QLDA.Application.DanhMucManHinhs.Commands;

public record DanhMucManHinhUpdateCommand(DanhMucManHinhUpdateDto Dto) : IRequest<DanhMucManHinh>;

internal class DanhMucManHinhUpdateCommandHandler : IRequestHandler<DanhMucManHinhUpdateCommand, DanhMucManHinh> {
    private readonly IRepository<DanhMucManHinh, int> DanhMucManHinh;
    private readonly Serilog.ILogger _logger = Serilog.Log.ForContext<DanhMucManHinhUpdateCommandHandler>();

    public DanhMucManHinhUpdateCommandHandler(IServiceProvider serviceProvider) {
        DanhMucManHinh = serviceProvider.GetRequiredService<IRepository<DanhMucManHinh, int>>();
    }

    public async Task<DanhMucManHinh> Handle(DanhMucManHinhUpdateCommand request, CancellationToken cancellationToken = default) {
        await ValidateAsync(request, cancellationToken);

        var entity = await DanhMucManHinh.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Dto.Id, cancellationToken);
        ManagedException.ThrowIfNull(entity);

        entity.Update(request.Dto);

        await DanhMucManHinh.UpdateAsync(entity, cancellationToken);
        await DanhMucManHinh.UnitOfWork.SaveChangesAsync(cancellationToken);
        return entity!;
    }
    #region  Private helper methods

    private async Task ValidateAsync(DanhMucManHinhUpdateCommand request, CancellationToken cancellationToken = default) {
        ManagedException.ThrowIf(
            when: await DanhMucManHinh.GetQueryableSet().AnyAsync(e => e.Id != request.Dto.Id && e.Ten == request.Dto.Ten, cancellationToken: cancellationToken),
            message: "Tên không được trùng");
    }



    #endregion
}