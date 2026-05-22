using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common;
using QLDA.Application.QuyetDinhDieuChinhs.DTOs;
using QLDA.Domain.Constants;

namespace QLDA.Application.QuyetDinhDieuChinhs.Commands;

/// <summary>
/// Tạo mới quyết định điều chỉnh
/// </summary>
public record QuyetDinhDieuChinhInsertCommand(QuyetDinhDieuChinhInsertDto Dto) : IRequest<int>;

internal class QuyetDinhDieuChinhInsertCommandHandler : IRequestHandler<QuyetDinhDieuChinhInsertCommand, int>
{
    private readonly IRepository<QuyetDinhDieuChinh, Guid> _repository;
    private readonly IRepository<ThongTinDieuChinhChiPhi, Guid> _chiPhiRepository;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository;
    private readonly IUserProvider _userProvider;
    private readonly IUnitOfWork _unitOfWork;

    public QuyetDinhDieuChinhInsertCommandHandler(IServiceProvider serviceProvider)
    {
        _repository = serviceProvider.GetRequiredService<IRepository<QuyetDinhDieuChinh, Guid>>();
        _chiPhiRepository = serviceProvider.GetRequiredService<IRepository<ThongTinDieuChinhChiPhi, Guid>>();
        _statusRepository = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task<int> Handle(QuyetDinhDieuChinhInsertCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;

        var trangThaiDuThao = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.QuyetDinhDieuChinh.DuThao && s.Loai == PheDuyetEntityNames.QuyetDinhDieuChinh, cancellationToken);

        ManagedException.ThrowIfNull(trangThaiDuThao, "Không tìm thấy trạng thái 'Dự thảo'");

        var lan = await _repository.GetQueryableSet()
            .CountAsync(e => e.PheDuyetEntityId == dto.EntityId
                             && e.PheDuyetEntityName == dto.Entity
                             && !e.IsDeleted, cancellationToken) + 1;

        // var entity = dto.ToEntity();
        // entity.TrangThaiId = trangThaiDuThao.Id;
        // entity.Lan = lan;

        // await _repository.AddAsync(entity, cancellationToken);

        // if (dto.ChiPhi != null)
        // {
        //     var chiPhi = dto.ChiPhi.ToEntity(entity.Id);
        //     await _chiPhiRepository.AddAsync(chiPhi, cancellationToken);
        // }

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}