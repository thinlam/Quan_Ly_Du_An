using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.KeHoachTrienKhaiHangMucMappings;
using QLDA.Application.KeHoachTrienKhaiHangMucs.DTOs;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.KeHoachTrienKhaiHangMucs.Commands;

public record KeHoachTrienKhaiHangMucInsertCommand(KeHoachTrienKhaiHangMuc entity) : IRequest<KeHoachTrienKhaiHangMuc>;

internal class KeHoachTrienKhaiHangMucInsertCommandHandler : IRequestHandler<KeHoachTrienKhaiHangMucInsertCommand, KeHoachTrienKhaiHangMuc> {
    private readonly IRepository<KeHoachTrienKhaiHangMuc, Guid> _repo;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepo;
    private readonly IUnitOfWork _unitOfWork;

    public KeHoachTrienKhaiHangMucInsertCommandHandler(IServiceProvider serviceProvider) {
        _repo = serviceProvider.GetRequiredService<IRepository<KeHoachTrienKhaiHangMuc, Guid>>();
        _statusRepo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _unitOfWork = _repo.UnitOfWork;
    }

    public async Task<KeHoachTrienKhaiHangMuc> Handle(KeHoachTrienKhaiHangMucInsertCommand request,
        CancellationToken cancellationToken = default) {
        var trangThaiDuThao = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == "DT" && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);
        // TẠI QUERY HANDLER: Phải Include danh sách này lên trước khi gọi Sync
      
        var entity = request.entity;
        entity.TrangThaiId = trangThaiDuThao?.Id;
        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await _repo.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        //entity.SyncDonViTuVan(request.Dto.DonViTuVans);
        //await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);
        return entity;
    }
}
