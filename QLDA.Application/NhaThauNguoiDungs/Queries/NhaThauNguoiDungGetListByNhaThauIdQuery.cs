using Microsoft.EntityFrameworkCore;
using QLDA.Application.NhaThauNguoiDungs.DTOs;

namespace QLDA.Application.NhaThauNguoiDungs.Queries;

/// <summary>
/// Get list of NguoiDung by NhaThauId
/// </summary>
public class NhaThauNguoiDungGetListByNhaThauIdQuery : IRequest<List<NhaThauNguoiDungDto>> {
    public Guid NhaThauId { get; set; }
}

internal class NhaThauNguoiDungGetListByNhaThauIdQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<NhaThauNguoiDungGetListByNhaThauIdQuery, List<NhaThauNguoiDungDto>> {
    private readonly IRepository<NhaThauNguoiDung, int> _repository =
        serviceProvider.GetRequiredService<IRepository<NhaThauNguoiDung, int>>();
    private readonly IRepository<UserMaster, long> _userRepository =
        serviceProvider.GetRequiredService<IRepository<UserMaster, long>>();

    public async Task<List<NhaThauNguoiDungDto>> Handle(NhaThauNguoiDungGetListByNhaThauIdQuery request,
        CancellationToken cancellationToken = default) {

        var nhaThauNguoiDungs = await _repository.GetQueryableSet()
            .AsNoTracking()
            .Where(e => e.NhaThauId == request.NhaThauId)
            .ToListAsync(cancellationToken);

        var nguoiDungIds = nhaThauNguoiDungs.Select(e => e.NguoiDungId).ToList();

        var users = await _userRepository.GetQueryableSet()
            .AsNoTracking()
            .Where(u => nguoiDungIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, cancellationToken);

        var result = nhaThauNguoiDungs.Select(e => new NhaThauNguoiDungDto {
            Id = e.Id,
            NhaThauId = e.NhaThauId,
            NguoiDungId = e.NguoiDungId,
            TenNguoiDung = users.TryGetValue(e.NguoiDungId, out var user) ? user.HoTen : null,
            UserName = user?.UserName
        }).ToList();

        return result;
    }
}
