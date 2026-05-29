using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.TrienKhaiKeHoachLCNTMappings;
using QLDA.Application.TrienKhaiKeHoachLCNTs.DTOs;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.TrienKhaiKeHoachLCNTs.Commands;

public record TrienKhaiKeHoachLCNTInsertCommand(TrienKhaiKeHoachLCNT Dto) : IRequest<TrienKhaiKeHoachLCNT>;

internal class TrienKhaiKeHoachLCNTInsertCommandHandler : IRequestHandler<TrienKhaiKeHoachLCNTInsertCommand, TrienKhaiKeHoachLCNT> {
    private readonly IRepository<TrienKhaiKeHoachLCNT, Guid> _repo;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepo;
    private readonly IUnitOfWork _unitOfWork;

    public TrienKhaiKeHoachLCNTInsertCommandHandler(IServiceProvider serviceProvider) {
        _repo = serviceProvider.GetRequiredService<IRepository<TrienKhaiKeHoachLCNT, Guid>>();
        _statusRepo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _unitOfWork = _repo.UnitOfWork;
    }

    public async Task<TrienKhaiKeHoachLCNT> Handle(TrienKhaiKeHoachLCNTInsertCommand request,
        CancellationToken cancellationToken = default) {
        var trangThaiDuThao = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == "DT" && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);
        // TẠI QUERY HANDLER: Phải Include danh sách này lên trước khi gọi Sync
      
        var entity = request.Dto;

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await _repo.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    //    entity.SyncDonViTuVan(request.Dto.DonViTuVans);
        //await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);
        return entity;
    }
}
