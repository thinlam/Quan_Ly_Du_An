using BuildingBlocks.CrossCutting.ExtensionMethods;
using BuildingBlocks.Domain.Providers;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.BaoCaoKetQuaKhaoSats.Commands;

public record BaoCaoKetQuaKhaoSatTrinhCommand(Guid Id, string? NoiDung = null) : IRequest<int>;

internal class BaoCaoKetQuaKhaoSatTrinhCommandHandler
    : IRequestHandler<BaoCaoKetQuaKhaoSatTrinhCommand, int>
{
    private readonly IRepository<BaoCaoKetQuaKhaoSat, Guid> _repository;
    private readonly IRepository<PheDuyetHistory, Guid> _historyRepository;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository;
    private readonly IUserProvider _userProvider;
    private readonly IUnitOfWork _unitOfWork;

    public BaoCaoKetQuaKhaoSatTrinhCommandHandler(IServiceProvider serviceProvider)
    {
        _repository = serviceProvider.GetRequiredService<IRepository<BaoCaoKetQuaKhaoSat, Guid>>();
        _historyRepository = serviceProvider.GetRequiredService<IRepository<PheDuyetHistory, Guid>>();
        _statusRepository = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task<int> Handle(BaoCaoKetQuaKhaoSatTrinhCommand request, CancellationToken cancellationToken)
    {
        var trangThaiDuThao = await _statusRepository
            .GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s =>
                s.Ma == TrangThaiPheDuyetCodes.DeXuatMacDinh.DuThao &&
                s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);
        var trangThaiTraLai = await _statusRepository
            .GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s =>
                s.Ma == TrangThaiPheDuyetCodes.DeXuatMacDinh.TraLai &&
                s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);
        var trangThaiDaTrinh = await _statusRepository
            .GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s =>
                s.Ma == TrangThaiPheDuyetCodes.DeXuatMacDinh.DaTrinh &&
                s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);

        ManagedException.ThrowIfNull(trangThaiDaTrinh, "Không tìm thấy trạng thái 'Đã trình'");

        var entity = await _repository.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
        ManagedException.ThrowIfNull(entity, "Không tìm thấy báo cáo kết quả khảo sát");

        if (entity.TrangThaiId != null &&
            entity.TrangThaiId != trangThaiDuThao?.Id &&
            entity.TrangThaiId != trangThaiTraLai?.Id)
        {
            throw new ManagedException("Chỉ có thể trình khi trạng thái là Dự thảo hoặc Trả lại");
        }

        entity.TrangThaiId = trangThaiDaTrinh.Id;
        entity.NgayTrinh = DateOnly.FromDateTime(DateTime.UtcNow).ToStartOfDayUtc();

        var history = new PheDuyetHistory
        {
            Id = Guid.NewGuid(),
            EntityName = PheDuyetEntityNames.BaoCaoKetQuaKhaoSat,
            EntityId = entity.Id,
            DuAnId = entity.DuAnId,
            NguoiXuLyId = _userProvider.Info.UserID,
            TrangThaiId = trangThaiDaTrinh.Id,
            NoiDung = request.NoiDung,
            NgayXuLy = DateTimeOffset.UtcNow
        };

        await _historyRepository.AddAsync(history, cancellationToken);
        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
