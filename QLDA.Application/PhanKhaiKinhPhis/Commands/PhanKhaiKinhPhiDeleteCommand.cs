using Microsoft.EntityFrameworkCore;

namespace QLDA.Application.PhanKhaiKinhPhis.Commands;

public record PhanKhaiKinhPhiDeleteCommand(Guid Id) : IRequest<int>;

internal class PhanKhaiKinhPhiDeleteCommandHandler : IRequestHandler<PhanKhaiKinhPhiDeleteCommand, int> {
    private readonly IRepository<PhanKhaiKinhPhi, Guid> _repo;
    private readonly IUnitOfWork _unitOfWork;

    public PhanKhaiKinhPhiDeleteCommandHandler(IServiceProvider serviceProvider) {
        _repo = serviceProvider.GetRequiredService<IRepository<PhanKhaiKinhPhi, Guid>>();
        _unitOfWork = _repo.UnitOfWork;
    }

    public async Task<int> Handle(PhanKhaiKinhPhiDeleteCommand request, CancellationToken cancellationToken = default) {
        var entity = await _repo.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
        ManagedException.ThrowIfNull(entity);

        entity.IsDeleted = true;
        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
