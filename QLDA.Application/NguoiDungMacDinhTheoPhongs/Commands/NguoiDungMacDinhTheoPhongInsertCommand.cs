using System.Data;
using BuildingBlocks.Domain.Entities;
using QLDA.Application.NguoiDungMacDinhTheoPhongs.DTOs;
using QLDA.Application.NguoiDungMacDinhTheoPhongs.Queries;

namespace QLDA.Application.NguoiDungMacDinhTheoPhongs.Commands;

public record NguoiDungMacDinhTheoPhongInsertCommand(NguoiDungMacDinhTheoPhongCreateDto Dto)
    : IRequest<NguoiDungMacDinhTheoPhongDto>;

internal class NguoiDungMacDinhTheoPhongInsertCommandHandler(IServiceProvider serviceProvider)
    : IRequestHandler<NguoiDungMacDinhTheoPhongInsertCommand, NguoiDungMacDinhTheoPhongDto>
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
        NguoiDungMacDinhTheoPhongInsertCommand request,
        CancellationToken cancellationToken = default)
    {
        var dto = request.Dto;
        NguoiDungMacDinhTheoPhongValidation.EnsureRequired(dto.PhongBanId, dto.NguoiDungId);
        await NguoiDungMacDinhTheoPhongValidation.EnsureReferencesExistAsync(
            dto.PhongBanId, dto.NguoiDungId, _dmDonVi, _userMaster, cancellationToken);
        await NguoiDungMacDinhTheoPhongValidation.EnsureNotDuplicateAsync(
            dto.PhongBanId, dto.NguoiDungId, _repository, cancellationToken);

        var entity = new NguoiDungMacDinhTheoPhong
        {
            PhongBanId = dto.PhongBanId,
            NguoiDungId = dto.NguoiDungId
        };

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return await _mediator.Send(new NguoiDungMacDinhTheoPhongGetByIdQuery(entity.Id), cancellationToken);
    }
}
