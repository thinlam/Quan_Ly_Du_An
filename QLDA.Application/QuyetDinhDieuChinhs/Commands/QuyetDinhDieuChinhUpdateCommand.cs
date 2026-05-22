using BuildingBlocks.Domain.Providers;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common;
using QLDA.Application.QuyetDinhDieuChinhs.DTOs;
using QLDA.Domain.Constants;

namespace QLDA.Application.QuyetDinhDieuChinhs.Commands;

/// <summary>
/// Cập nhật quyết định điều chỉnh - chỉ khi ở trạng thái Nháp hoặc Trả lại
/// </summary>
public record QuyetDinhDieuChinhUpdateCommand(QuyetDinhDieuChinhUpdateDto Dto) : IRequest<int>;

internal class QuyetDinhDieuChinhUpdateCommandHandler : IRequestHandler<QuyetDinhDieuChinhUpdateCommand, int> {
    private readonly IRepository<QuyetDinhDieuChinh, Guid> _repository;
    private readonly IRepository<ThongTinDieuChinhChiPhi, Guid> _chiPhiRepository;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository;
    private readonly IUserProvider _userProvider;
    private readonly IUnitOfWork _unitOfWork;

    public QuyetDinhDieuChinhUpdateCommandHandler(IServiceProvider serviceProvider) {
        _repository = serviceProvider.GetRequiredService<IRepository<QuyetDinhDieuChinh, Guid>>();
        _chiPhiRepository = serviceProvider.GetRequiredService<IRepository<ThongTinDieuChinhChiPhi, Guid>>();
        _statusRepository = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task<int> Handle(QuyetDinhDieuChinhUpdateCommand request, CancellationToken cancellationToken) {
        var dto = request.Dto;

        var trangThaiDuThao = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.QuyetDinhDieuChinh.DuThao && s.Loai == PheDuyetEntityNames.QuyetDinhDieuChinh, cancellationToken);
        var trangThaiTraLai = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.QuyetDinhDieuChinh.TraLai && s.Loai == PheDuyetEntityNames.QuyetDinhDieuChinh, cancellationToken);

        var entity = await _repository.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == dto.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity, "Không tìm thấy quyết định điều chỉnh");

        if (entity.TrangThaiId != trangThaiDuThao?.Id && entity.TrangThaiId != trangThaiTraLai?.Id) {
            throw new ManagedException("Chỉ có thể cập nhật khi trạng thái là Dự thảo");
        }

        // entity.Update(dto);

        // var existingChiPhi = await _chiPhiRepository.GetQueryableSet()
        //     .FirstOrDefaultAsync(c => c.QuyetDinhDieuChinhId == entity.Id, cancellationToken);

        // if (dto.ChiPhi != null) {
        //     if (existingChiPhi != null) {
        //         existingChiPhi.Update(dto.ChiPhi);
        //     }
        //     else {
        //         var chiPhi = dto.ChiPhi.ToEntity(entity.Id);
        //         await _chiPhiRepository.AddAsync(chiPhi, cancellationToken);
        //     }
        // }
        // else if (existingChiPhi != null) {
        //     existingChiPhi.IsDeleted = true;
        // }

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}