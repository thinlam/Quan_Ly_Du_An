using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.KetQuaTrungThaus.DTOs;
using QLDA.Domain.Entities;

namespace QLDA.Application.KetQuaTrungThaus.Commands;

public record KetQuaTrungThauInsertCommand(KetQuaTrungThauInsertDto Dto) : IRequest<KetQuaTrungThau>;

internal class KetQuaTrungThauInsertCommandHandler : IRequestHandler<KetQuaTrungThauInsertCommand, KetQuaTrungThau> {
    private readonly IRepository<KetQuaTrungThau, Guid> KetQuaTrungThau;
    private readonly IRepository<DuAn, Guid> DuAn;
    private readonly IRepository<DuAnBuoc, int> _duAnBuocRepo;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationContext _authContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly Serilog.ILogger _logger = Serilog.Log.ForContext<KetQuaTrungThauInsertCommandHandler>();

    public KetQuaTrungThauInsertCommandHandler(IServiceProvider serviceProvider) {
        KetQuaTrungThau = serviceProvider.GetRequiredService<IRepository<KetQuaTrungThau, Guid>>();
        DuAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
        _duAnBuocRepo = serviceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _unitOfWork = KetQuaTrungThau.UnitOfWork;
    }

    public async Task<KetQuaTrungThau> Handle(KetQuaTrungThauInsertCommand request, CancellationToken cancellationToken = default) {

        await ValidateAsync(request, cancellationToken);

        if (request.Dto.BuocId.HasValue)
        {
            var buoc = await _duAnBuocRepo.GetQueryableSet()
                .Include(e => e.DuAn)
                .Include(e => e.DuAnBuocPhongBanPhoiHops)
                .FirstOrDefaultAsync(e => e.Id == request.Dto.BuocId.Value, cancellationToken);
            if (buoc != null && !await _auth.CanExecuteStepAsync(buoc, _authContext, cancellationToken))
                throw new ManagedException("Phòng ban không có quyền thao tác bước này");
        }

        var entity = request.Dto.ToEntity();

        if (_unitOfWork.HasTransaction) {
            await InsertAsync(entity, cancellationToken);
        } else {
            using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken); await InsertAsync(entity, cancellationToken);
            await InsertAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }


        return entity;

    }

    #region  Private helper methods

    private async Task ValidateAsync(KetQuaTrungThauInsertCommand request, CancellationToken cancellationToken) {
        ManagedException.ThrowIf(!await DuAn.GetQueryableSet().AnyAsync(e => e.Id == request.Dto.DuAnId, cancellationToken: cancellationToken),
           "Không tồn tại dự án");
        //ManagedException.ThrowIf(
        //    when: await KetQuaTrungThau.GetQueryableSet().AnyAsync(e => e.SoQuyetDinh == request.Dto.SoQuyetDinh, cancellationToken: cancellationToken),
        //    message: "Số quyết định đã tồn tại"
        //);
        ManagedException.ThrowIf(
            when: await KetQuaTrungThau.GetQueryableSet().AnyAsync(e => e.GoiThauId == request.Dto.GoiThauId, cancellationToken: cancellationToken),
            message: "Gói thầu đã có kết quả trúng thầu"
        );
    }

    private async Task InsertAsync(KetQuaTrungThau entity, CancellationToken cancellationToken) {
        await KetQuaTrungThau.AddAsync(entity, cancellationToken);
    }

    #endregion
}
