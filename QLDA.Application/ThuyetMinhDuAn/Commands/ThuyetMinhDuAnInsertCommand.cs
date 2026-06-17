using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.ThuyetMinhDuAns.DTOs;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.ThuyetMinhDuAns.Commands;

public record ThuyetMinhDuAnInsertCommand(ThuyetMinhDuAn Dto) : IRequest<ThuyetMinhDuAn>;

internal class ThuyetMinhDuAnInsertCommandHandler : IRequestHandler<ThuyetMinhDuAnInsertCommand, ThuyetMinhDuAn>
{
    private readonly IRepository<ThuyetMinhDuAn, Guid> _repo;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepo;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationContext _authContext;
    private readonly IUnitOfWork _unitOfWork;

    public ThuyetMinhDuAnInsertCommandHandler(IServiceProvider serviceProvider)
    {
        _repo = serviceProvider.GetRequiredService<IRepository<ThuyetMinhDuAn, Guid>>();
        _statusRepo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _unitOfWork = _repo.UnitOfWork;
    }

    public async Task<ThuyetMinhDuAn> Handle(ThuyetMinhDuAnInsertCommand request, CancellationToken cancellationToken = default)
    {
        await _auth.EnsureCanExecuteStepAsync(request.Dto.BuocId, _authContext, cancellationToken);

        // Auto-assign Dự thảo status
        var trangThaiDuThao = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == "DT" && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);

        var entity = new ThuyetMinhDuAn
        {
            DuAnId = request.Dto.DuAnId,
            BuocId = request.Dto.BuocId,
            So = request.Dto.So,
            NgayTrinh = request.Dto.NgayTrinh,
            TrichYeu = request.Dto.TrichYeu,
            //KetQuaThamDinh = request.Dto.KetQuaThamDinh,//chỉ user phòng KH-tc
            //TrangThaiThamDinhId = request.Dto.TrangThaiThamDinhId,
            TrangThaiId = trangThaiDuThao?.Id,
        };

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await _repo.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return entity;
    }
}
