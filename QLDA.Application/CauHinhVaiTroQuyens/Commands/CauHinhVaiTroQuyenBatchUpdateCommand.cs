using Microsoft.EntityFrameworkCore;
using QLDA.Application.CauHinhVaiTroQuyens.DTOs;

namespace QLDA.Application.CauHinhVaiTroQuyens.Commands;

public record CauHinhVaiTroQuyenBatchUpdateCommand(CauHinhVaiTroQuyenUpdateDto Dto) : IRequest<int>;

internal class CauHinhVaiTroQuyenBatchUpdateCommandHandler : IRequestHandler<CauHinhVaiTroQuyenBatchUpdateCommand, int> {
    private readonly IRepository<CauHinhVaiTroQuyen, int> _repo;
    private readonly IUnitOfWork _unitOfWork;

    public CauHinhVaiTroQuyenBatchUpdateCommandHandler(IServiceProvider serviceProvider) {
        _repo = serviceProvider.GetRequiredService<IRepository<CauHinhVaiTroQuyen, int>>();
        _unitOfWork = _repo.UnitOfWork;
    }

    public async Task<int> Handle(CauHinhVaiTroQuyenBatchUpdateCommand request, CancellationToken cancellationToken) {
        var ids = request.Dto.Items.Select(i => i.Id).ToList();
        var entities = await _repo.GetQueryableSet(OnlyUsed: false, OnlyNotDeleted: false)
            .Where(c => ids.Contains(c.Id))
            .ToListAsync(cancellationToken);

        foreach (var entity in entities) {
            var item = request.Dto.Items.FirstOrDefault(i => i.Id == entity.Id);
            if (item != null) {
                entity.KichHoat = item.KichHoat;
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return entities.Count;
    }
}
