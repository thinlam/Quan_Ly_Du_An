using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.KetQuaTrungThaus.DTOs;

namespace QLDA.Application.KetQuaTrungThaus.Commands;

public record KetQuaTrungThauUpdateCommand(KetQuaTrungThauUpdateDto Dto) : IRequest<KetQuaTrungThau>;

internal class KetQuaTrungThauUpdateCommandHandler : IRequestHandler<KetQuaTrungThauUpdateCommand, KetQuaTrungThau> {
    private readonly IRepository<KetQuaTrungThau, Guid> KetQuaTrungThau;
    private readonly IRepository<DuAn, Guid> DuAn;
    private readonly IUnitOfWork _unitOfWork;
    private readonly Serilog.ILogger _logger = Serilog.Log.ForContext<KetQuaTrungThauUpdateCommandHandler>();

    public KetQuaTrungThauUpdateCommandHandler(IServiceProvider serviceProvider) {
        KetQuaTrungThau = serviceProvider.GetRequiredService<IRepository<KetQuaTrungThau, Guid>>();
        DuAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
        _unitOfWork = KetQuaTrungThau.UnitOfWork;
    }

    public async Task<KetQuaTrungThau> Handle(KetQuaTrungThauUpdateCommand request, CancellationToken cancellationToken = default) {
        await ValidateAsync(request, cancellationToken);

        var entity = await KetQuaTrungThau.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Dto.Id, cancellationToken);
        ManagedException.ThrowIfNull(entity);

        entity.Update(request.Dto);

        if (_unitOfWork.HasTransaction) {
            await UpdateAsync(entity, cancellationToken);
        } else {
            using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
            await UpdateAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        return entity;
    }
    #region  Private helper methods

    private async Task ValidateAsync(KetQuaTrungThauUpdateCommand request, CancellationToken cancellationToken) {
        //ManagedException.ThrowIf(
        //    when: await KetQuaTrungThau.GetQueryableSet().AnyAsync(e => e.Id != request.Dto.Id && e.SoQuyetDinh!.ToLower() == request.Dto.SoQuyetDinh!.ToLower(), cancellationToken: cancellationToken),
        //    message: "Số quyết định đã tồn tại"
        //);
        ManagedException.ThrowIf(
            when: await KetQuaTrungThau.GetQueryableSet().AnyAsync(e => e.Id != request.Dto.Id && e.GoiThauId == request.Dto.GoiThauId, cancellationToken: cancellationToken),
            message: "Gói thầu đã có kết quả trúng thầu"
        );
    }
    private async Task UpdateAsync(KetQuaTrungThau entity, CancellationToken cancellationToken) {
        await KetQuaTrungThau.UpdateAsync(entity, cancellationToken);
    }

    #endregion
}