using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.ToTrinhThamDinhNhaThaus.DTOs;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.ToTrinhThamDinhNhaThaus.Commands;

public record ToTrinhThamDinhNhaThauInsertCommand(ToTrinhThamDinhNhaThau Dto) : IRequest<Domain.Entities.ToTrinhThamDinhNhaThau>;

internal class ToTrinhThamDinhNhaThauInsertCommandHandler : IRequestHandler<ToTrinhThamDinhNhaThauInsertCommand, Domain.Entities.ToTrinhThamDinhNhaThau> {
    private readonly IRepository<Domain.Entities.ToTrinhThamDinhNhaThau, Guid> _repo;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepo;
    private readonly IUnitOfWork _unitOfWork;

    public ToTrinhThamDinhNhaThauInsertCommandHandler(IServiceProvider serviceProvider) {
        _repo = serviceProvider.GetRequiredService<IRepository<Domain.Entities.ToTrinhThamDinhNhaThau, Guid>>();
        _statusRepo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _unitOfWork = _repo.UnitOfWork;
    }

    public async Task<Domain.Entities.ToTrinhThamDinhNhaThau> Handle(ToTrinhThamDinhNhaThauInsertCommand request,
        CancellationToken cancellationToken = default) {
        var trangThaiDuThao = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == "DT" && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);

        var entity = new Domain.Entities.ToTrinhThamDinhNhaThau {
            So = request.Dto.So,
            BuocId = request.Dto.BuocId,
            DuAnId = request.Dto.DuAnId,
            NgayTrinh = request.Dto.NgayTrinh,
            TrichYeu = request.Dto.TrichYeu,
            TrangThaiDangTaiId = request.Dto.TrangThaiDangTaiId,
            TrangThaiThamDinhId = request.Dto.TrangThaiThamDinhId,
            TrangThaiId = trangThaiDuThao?.Id
           
        };

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await _repo.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        entity.SyncNhaThauIds(entity.Id,request.Dto.NhaThaus);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);
        return entity;
    }
}
