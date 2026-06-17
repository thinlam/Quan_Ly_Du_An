using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.GoiThaus.DTOs;

namespace QLDA.Application.GoiThaus.Commands;

public record GoiThauInsertCommand(GoiThauInsertDto Dto) : IRequest<GoiThau>;

internal class GoiThauInsertCommandHandler : IRequestHandler<GoiThauInsertCommand, GoiThau> {
    private readonly IRepository<GoiThau, Guid> GoiThau;
    private readonly IRepository<DuAn, Guid> DuAn;
    private readonly IRepository<DanhMucLoaiHopDong, int> DanhMucLoaiHopDong;
    private readonly IRepository<DanhMucHinhThucLuaChonNhaThau, int> DanhMucHinhThucLuaChonNhaThau;
    private readonly IRepository<DanhMucPhuongThucLuaChonNhaThau, int> DanhMucPhuongThucLuaChonNhaThau;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationContext _authContext;

    private readonly IUnitOfWork _unitOfWork;
    private readonly Serilog.ILogger _logger = Serilog.Log.ForContext<GoiThauInsertCommandHandler>();

    public GoiThauInsertCommandHandler(IServiceProvider serviceProvider) {
        GoiThau = serviceProvider.GetRequiredService<IRepository<GoiThau, Guid>>();
        DuAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
        DanhMucLoaiHopDong = serviceProvider.GetRequiredService<IRepository<DanhMucLoaiHopDong, int>>();
        DanhMucHinhThucLuaChonNhaThau = serviceProvider.GetRequiredService<IRepository<DanhMucHinhThucLuaChonNhaThau, int>>();
        DanhMucPhuongThucLuaChonNhaThau = serviceProvider.GetRequiredService<IRepository<DanhMucPhuongThucLuaChonNhaThau, int>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _unitOfWork = GoiThau.UnitOfWork;
    }

    public async Task<GoiThau> Handle(GoiThauInsertCommand request, CancellationToken cancellationToken = default) {

        await ValidateAsync(request, cancellationToken);

        // Check step authorization
        await _auth.EnsureCanExecuteStepAsync(request.Dto.BuocId, _authContext, cancellationToken);

        var entity = request.Dto.ToEntity();

        if (_unitOfWork.HasTransaction) {
            await InsertAsync(entity, cancellationToken);
        } else {
            using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
            await InsertAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }


        return entity;

    }

    #region  Private helper methods

    private async Task ValidateAsync(GoiThauInsertCommand request, CancellationToken cancellationToken) {
        ManagedException.ThrowIf(!await DuAn.GetQueryableSet().AnyAsync(e => e.Id == request.Dto.DuAnId, cancellationToken: cancellationToken),
           "Không tồn tại dự án");
        ManagedException.ThrowIf(
            request.Dto.LoaiHopDongId > 0 && !await DanhMucLoaiHopDong.GetQueryableSet()
                .AnyAsync(e => e.Id == request.Dto.LoaiHopDongId, cancellationToken),
            "Không tồn tại loại hợp đồng này");
        ManagedException.ThrowIf(
            request.Dto.HinhThucLuaChonNhaThauId > 0 && !await DanhMucHinhThucLuaChonNhaThau.GetQueryableSet()
                .AnyAsync(e => e.Id == request.Dto.HinhThucLuaChonNhaThauId, cancellationToken),
            "Không tồn tại hình thức chọn gói thầu này");
        ManagedException.ThrowIf(
            request.Dto.PhuongThucLuaChonNhaThauId > 0 && !await DanhMucPhuongThucLuaChonNhaThau.GetQueryableSet()
                .AnyAsync(e => e.Id == request.Dto.PhuongThucLuaChonNhaThauId, cancellationToken),
            "Không tồn tại phương thức chọn gói thầu này");
    }

    private async Task InsertAsync(GoiThau entity, CancellationToken cancellationToken) {
        await GoiThau.AddAsync(entity, cancellationToken);
    }

    #endregion
}