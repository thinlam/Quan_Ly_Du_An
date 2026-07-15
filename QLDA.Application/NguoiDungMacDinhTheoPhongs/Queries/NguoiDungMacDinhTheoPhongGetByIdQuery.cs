using Microsoft.EntityFrameworkCore;
using QLDA.Application.NguoiDungMacDinhTheoPhongs.DTOs;

namespace QLDA.Application.NguoiDungMacDinhTheoPhongs.Queries;

public record NguoiDungMacDinhTheoPhongGetByIdQuery(Guid Id) : IRequest<NguoiDungMacDinhTheoPhongDto>;

internal class NguoiDungMacDinhTheoPhongGetByIdQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<NguoiDungMacDinhTheoPhongGetByIdQuery, NguoiDungMacDinhTheoPhongDto>
{
    private readonly IRepository<NguoiDungMacDinhTheoPhong, Guid> _repository =
        serviceProvider.GetRequiredService<IRepository<NguoiDungMacDinhTheoPhong, Guid>>();

    private readonly IRepository<DmDonVi, long> _dmDonVi =
        serviceProvider.GetRequiredService<IRepository<DmDonVi, long>>();

    private readonly IRepository<UserMaster, long> _userMaster =
        serviceProvider.GetRequiredService<IRepository<UserMaster, long>>();

    public async Task<NguoiDungMacDinhTheoPhongDto> Handle(
        NguoiDungMacDinhTheoPhongGetByIdQuery request,
        CancellationToken cancellationToken = default)
    {
        var dmDonVi = _dmDonVi.GetQueryableSet().AsNoTracking();
        var userMaster = _userMaster.GetQueryableSet().AsNoTracking();

        var dto = await _repository.GetQueryableSet()
            .AsNoTracking()
            .Where(e => e.Id == request.Id)
            .SelectDto(dmDonVi, userMaster)
            .FirstOrDefaultAsync(cancellationToken);

        ManagedException.ThrowIfNull(dto, "Không tìm thấy dữ liệu");
        return dto;
    }
}
