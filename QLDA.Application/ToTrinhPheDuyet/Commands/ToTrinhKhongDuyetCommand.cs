using BuildingBlocks.Domain.Providers;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities;
using QLDA.Domain.Entities.DanhMuc;
using System.Reflection;

namespace QLDA.Application.ToTrinhPheDuyets.Commands;

/// <summary>
/// Trình quyết định điều chỉnh - chỉ phòng KH-TC (PhongBanId = 219)
/// </summary>
public record ToTrinhKhongDuyetCommand(Guid Id, string Loai, string? NoiDung = null) : IRequest<int>;
// chu thich: Các tờ trình gọi api này thì ko cần duyệt. Trình là tính duyệt
internal class ToTrinhKhongDuyetCommandHandler : IRequestHandler<ToTrinhKhongDuyetCommand, int>
{
    private readonly DbContext _dbContext;
    private readonly IRepository<ToTrinhPheDuyet, Guid> _repository;
    private readonly IRepository<KeHoachLuaChonNhaThau, Guid>_keHoachRepo;
    private readonly IRepository<QuyetDinhDuyetDuToan, Guid> _quyetDinhDuyetDuToan;
    private readonly IRepository<PheDuyetHistory, Guid> _historyRepository;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationContext _authContext;
    private readonly IUserProvider _userProvider;
    private readonly IUnitOfWork _unitOfWork;

    public ToTrinhKhongDuyetCommandHandler(DbContext dbContext, IServiceProvider serviceProvider)
    {
        _dbContext = dbContext;
        _repository = serviceProvider.GetRequiredService<IRepository<ToTrinhPheDuyet, Guid>>();
        _keHoachRepo = serviceProvider.GetRequiredService<IRepository<KeHoachLuaChonNhaThau, Guid>>();
        _quyetDinhDuyetDuToan = serviceProvider.GetRequiredService<IRepository<QuyetDinhDuyetDuToan, Guid>>();
        _historyRepository = serviceProvider.GetRequiredService<IRepository<PheDuyetHistory, Guid>>();
        _statusRepository = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task<int> Handle(ToTrinhKhongDuyetCommand request, CancellationToken cancellationToken)
    {
        // entity này có 2 loại trạng thái là trạng thái đề xuất mặc định và trạng thái tờ trình ko cần duyệt( trình là xong)

       // bool isKhongDuyet = LoaiToTrinhKhongDuyetExtensions.ContainsDescription(request.Loai); allway true
        var loaiPheDuyet =PheDuyetEntityNames.ToTrinhKhongDuyet;
        var statuses = await _statusRepository.GetByLoaiAsync(loaiPheDuyet, cancellationToken);
        var statusDict = statuses.ToDictionary(x => x.Ma);

        var trangThaiDuThao = statusDict.GetValueOrDefault(TrangThaiPheDuyetCodes.DeXuatMacDinh.DuThao);
        var trangThaiTraLai = statusDict.GetValueOrDefault(TrangThaiPheDuyetCodes.DeXuatMacDinh.TraLai);
        var trangThaiDaTrinh = statusDict.GetValueOrDefault(TrangThaiPheDuyetCodes.DeXuatMacDinh.DaTrinh);

        ManagedException.ThrowIfNull(trangThaiDaTrinh, "Không tìm thấy trạng thái 'Đã trình'");


        string table = request.Loai;
        if (ToTrinhEntityNamesExtensions.ContainsEntity(request.Loai))
            table = "ToTrinhPheDuyet";

        var entityType = _dbContext.Model.GetEntityTypes()
                .FirstOrDefault(t => t.ClrType.Name == table)?.ClrType;

        ManagedException.ThrowIfNull(entityType, "Không tìm thấy quyết định/tờ trình cần thao tác");

        var entity = await _dbContext.FindAsync(entityType, new object[] { request.Id }, cancellationToken) as IApprovableEntity;
        if (entity == null)
        {
            throw new ManagedException("Không tìm thấy dữ liệu cần thao tác trong hệ thống!");
        }

        await _auth.EnsureCanExecuteStepAsync(entity.BuocId, _authContext, cancellationToken);

        // Validate: must be DT (Dự thảo) or TL (Trả lại) to transition to ĐTr (Đã trình)
        if (entity.TrangThaiId != trangThaiDuThao?.Id && entity.TrangThaiId != trangThaiTraLai?.Id)
        {
            throw new ManagedException("Chỉ có thể trình khi trạng thái là Dự thảo hoặc Trả lại!");
        }

        // 5. Cập nhật TrangThaiId (Dù là Model nào cũng chỉ tốn đúng 1 dòng này)
        entity.TrangThaiId = trangThaiDaTrinh.Id;

        // 6. Lưu lịch sử phê duyệt
        var history = new PheDuyetHistory
        {
            Id = Guid.NewGuid(),
            EntityName = request.Loai,
            EntityId = request.Id,
            DuAnId = entity.DuAnId,
            BuocId = entity.BuocId,
            NguoiXuLyId = _userProvider.Info.UserID,
            TrangThaiId = trangThaiDaTrinh.Id,
            NoiDung = request.NoiDung,
            NgayXuLy = DateTimeOffset.UtcNow
        };
        await _historyRepository.AddAsync(history);
        #region 
        // nếu là tờ trình kế hoạch lcnt  -> duyệt thì insert vào table KeHoachLuaChonNhaThau
        if (Enum.IsDefined(typeof(KeHoachLuaChonNhaThauLoai), request.Loai))
        {
            var entityKeHoach = await _repository.GetQueryableSet().FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
            ManagedException.ThrowIfNull(entityKeHoach, "Không tìm thấy kế hoạch cần cập nhật");
            var keHoach = new KeHoachLuaChonNhaThau
            {
                Id = Guid.NewGuid(),
                Ten = entityKeHoach.Ten,
                Loai = request.Loai,
                DuAnId= entityKeHoach.DuAnId,
                BuocId = entityKeHoach.BuocId
            };
            await _keHoachRepo.AddAsync(keHoach, cancellationToken);
        }
        #endregion
        // 7. Lưu thay đổi vào DB thông qua DbContext
        await _dbContext.SaveChangesAsync(cancellationToken);

        return 1;



    }


  
}
