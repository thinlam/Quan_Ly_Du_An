using BuildingBlocks.Domain.Providers;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.ToTrinhPheDuyets.Commands;

/// <summary>
/// Trình quyết định điều chỉnh - chỉ phòng KH-TC (PhongBanId = 219)
/// </summary>
public record ToTrinhPheDuyetTrinhCommand(Guid Id, string Loai,string? NoiDung = null) : IRequest<int>;

internal class ToTrinhPheDuyetTrinhCommandHandler : IRequestHandler<ToTrinhPheDuyetTrinhCommand, int> {
    private readonly IRepository<ToTrinhPheDuyet, Guid> _repository;
    private readonly IRepository<PheDuyetHistory, Guid> _historyRepository;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository;
    private readonly IUserProvider _userProvider;
    private readonly IUnitOfWork _unitOfWork;

    public ToTrinhPheDuyetTrinhCommandHandler(IServiceProvider serviceProvider) {
        _repository = serviceProvider.GetRequiredService<IRepository<ToTrinhPheDuyet, Guid>>();
        _historyRepository = serviceProvider.GetRequiredService<IRepository<PheDuyetHistory, Guid>>();
        _statusRepository = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task<int> Handle(ToTrinhPheDuyetTrinhCommand request, CancellationToken cancellationToken) {
        // Permission: KH-TC only (PhongBanId = 219)
        var phongBanId = _userProvider.Info.PhongBanID;

        bool isKhongDuyet = LoaiToTrinhKhongDuyetExtensions.ContainsDescription(request.Loai);
        var loaiPheDuyet = isKhongDuyet   ? PheDuyetEntityNames.ToTrinhKhongDuyet
                                          : PheDuyetEntityNames.DeXuatMacDinhStt;
        var statuses = await _statusRepository.GetByLoaiAsync(loaiPheDuyet, cancellationToken);
        var statusDict = statuses.ToDictionary(x => x.Ma);

        var trangThaiDuThao = statusDict.GetValueOrDefault(TrangThaiPheDuyetCodes.DeXuatMacDinh.DuThao);
        var trangThaiTraLai = statusDict.GetValueOrDefault(TrangThaiPheDuyetCodes.DeXuatMacDinh.TraLai);
        var trangThaiDaTrinh = statusDict.GetValueOrDefault(TrangThaiPheDuyetCodes.DeXuatMacDinh.DaTrinh);

        ManagedException.ThrowIfNull(trangThaiDaTrinh, "Không tìm thấy trạng thái 'Đã trình'");

        var entity = await _repository.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity, "Không tìm thấy quyết định/tờ trình cần thao tác");

        // Validate: must be DT (Dự thảo) or TL (Trả lại) to transition to ĐTr (Đã trình)
        if ( entity.TrangThaiId != trangThaiDuThao?.Id && entity.TrangThaiId != trangThaiTraLai?.Id) {
            throw new ManagedException("Chỉ có thể trình khi trạng thái là Dự thảo!");
        }

        entity.TrangThaiId = trangThaiDaTrinh.Id;
        // hien tai có 2 loai la PheDuyetKhaoSat va QuyetDinhKeHoachThue
        var history = new PheDuyetHistory {
            Id = Guid.NewGuid(),
            EntityName = request.Loai,
            EntityId = entity.Id,
            DuAnId = entity.DuAnId,
            NguoiXuLyId = _userProvider.Info.UserID,
            TrangThaiId = trangThaiDaTrinh.Id,
            NoiDung = request.NoiDung,
            NgayXuLy = DateTimeOffset.UtcNow
        };

        await _historyRepository.AddAsync(history);

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}