using Microsoft.EntityFrameworkCore;
using QLDA.Application.DanhMucManHinhs.DTOs;

namespace QLDA.Application.DanhMucManHinhs.Commands;

public record DanhMucManHinhInsertCommand(DanhMucManHinhInsertDto Dto) : IRequest<DanhMucManHinh>;

internal class DanhMucManHinhInsertCommandHandler(IServiceProvider serviceProvider) : IRequestHandler<DanhMucManHinhInsertCommand, DanhMucManHinh> {
    private readonly IRepository<DanhMucManHinh, int> DanhMucManHinh = serviceProvider.GetRequiredService<IRepository<DanhMucManHinh, int>>();
    private readonly Serilog.ILogger _logger = Serilog.Log.ForContext<DanhMucManHinhInsertCommandHandler>();

    public async Task<DanhMucManHinh> Handle(DanhMucManHinhInsertCommand request, CancellationToken cancellationToken = default) {

        await ValidateAsync(request, cancellationToken);

        var entity = request.Dto.ToEntity();

        await DanhMucManHinh.AddAsync(entity, cancellationToken);
        await DanhMucManHinh.UnitOfWork.SaveChangesAsync(cancellationToken);

        return entity!;
    }

    #region  Private helper methods

    private async Task ValidateAsync(DanhMucManHinhInsertCommand request, CancellationToken cancellationToken = default) {
        ManagedException.ThrowIf(
            when: await DanhMucManHinh.GetQueryableSet().AnyAsync(e => e.Ten == request.Dto.Ten, cancellationToken: cancellationToken),
            message: "Tên không được trùng");
    }



    #endregion
}