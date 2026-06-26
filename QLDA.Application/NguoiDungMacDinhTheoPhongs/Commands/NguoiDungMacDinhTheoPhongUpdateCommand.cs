using BuildingBlocks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.NguoiDungMacDinhTheoPhongs.DTOs;
using QLDA.Application.NguoiDungMacDinhTheoPhongs.Queries;

namespace QLDA.Application.NguoiDungMacDinhTheoPhongs.Commands;

public record NguoiDungMacDinhTheoPhongUpdateCommand(NguoiDungMacDinhTheoPhongUpdateDto Dto)
    : IRequest<NguoiDungMacDinhTheoPhongDto>;

internal class NguoiDungMacDinhTheoPhongUpdateCommandHandler(IServiceProvider serviceProvider)
    : IRequestHandler<NguoiDungMacDinhTheoPhongUpdateCommand, NguoiDungMacDinhTheoPhongDto>
{
    private readonly IRepository<NguoiDungMacDinhTheoPhong, Guid> _repository =
        serviceProvider.GetRequiredService<IRepository<NguoiDungMacDinhTheoPhong, Guid>>();

    private readonly IRepository<DmDonVi, long> _dmDonVi =
        serviceProvider.GetRequiredService<IRepository<DmDonVi, long>>();

    private readonly IRepository<UserMaster, long> _userMaster =
        serviceProvider.GetRequiredService<IRepository<UserMaster, long>>();

    private readonly IMediator _mediator = serviceProvider.GetRequiredService<IMediator>();
    private readonly IUnitOfWork _unitOfWork = serviceProvider.GetRequiredService<IRepository<NguoiDungMacDinhTheoPhong, Guid>>().UnitOfWork;

    public async Task<NguoiDungMacDinhTheoPhongDto> Handle(
        NguoiDungMacDinhTheoPhongUpdateCommand request,
        CancellationToken cancellationToken = default)
    {
        var dto = request.Dto;
        NguoiDungMacDinhTheoPhongValidation.EnsureRequired(dto.PhongBanId, dto.NguoiDungId);
        await NguoiDungMacDinhTheoPhongValidation.EnsureReferencesExistAsync(
            dto.PhongBanId, dto.NguoiDungId, _dmDonVi, _userMaster, cancellationToken);
        await NguoiDungMacDinhTheoPhongValidation.EnsureNotDuplicateAsync(
            dto.PhongBanId, dto.NguoiDungId, _repository, cancellationToken, dto.Id);

        var entity = await _repository.GetOrderedSet()
            .FirstOrDefaultAsync(e => e.Id == dto.Id, cancellationToken);
        ManagedException.ThrowIfNull(entity, "Không tìm thấy dữ liệu");

        entity.PhongBanId = dto.PhongBanId;
        entity.NguoiDungId = dto.NguoiDungId;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await _mediator.Send(new NguoiDungMacDinhTheoPhongGetByIdQuery(entity.Id), cancellationToken);
    }
}
