using Microsoft.EntityFrameworkCore;

namespace QLDA.Application.NguoiDungMacDinhTheoPhongs.Commands;

public record NguoiDungMacDinhTheoPhongDeleteCommand(Guid Id) : IRequest<bool>;

internal class NguoiDungMacDinhTheoPhongDeleteCommandHandler(IServiceProvider serviceProvider)
    : IRequestHandler<NguoiDungMacDinhTheoPhongDeleteCommand, bool>
{
    private readonly IRepository<NguoiDungMacDinhTheoPhong, Guid> _repository =
        serviceProvider.GetRequiredService<IRepository<NguoiDungMacDinhTheoPhong, Guid>>();

    private readonly IUnitOfWork _unitOfWork = serviceProvider.GetRequiredService<IRepository<NguoiDungMacDinhTheoPhong, Guid>>().UnitOfWork;

    public async Task<bool> Handle(
        NguoiDungMacDinhTheoPhongDeleteCommand request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetOrderedSet()
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
        ManagedException.ThrowIfNull(entity, "Không tìm thấy dữ liệu");

        entity.IsDeleted = true;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
