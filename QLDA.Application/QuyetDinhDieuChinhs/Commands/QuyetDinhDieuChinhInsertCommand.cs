using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common;
using QLDA.Application.QuyetDinhDieuChinhs.DTOs;
using QLDA.Domain.Constants;
using System.Data;

namespace QLDA.Application.QuyetDinhDieuChinhs.Commands;

/// <summary>
/// Tạo mới quyết định điều chỉnh
/// </summary>
public record QuyetDinhDieuChinhInsertCommand(QuyetDinhDieuChinhInsertDto Dto) : IRequest<QuyetDinhDieuChinh>;

internal class QuyetDinhDieuChinhInsertCommandHandler : IRequestHandler<QuyetDinhDieuChinhInsertCommand, QuyetDinhDieuChinh>
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

    public async Task<QuyetDinhDieuChinh> Handle(QuyetDinhDieuChinhInsertCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;

        var trangThaiDuThao = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DeXuatMacDinh.DuThao 
            && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);

        ManagedException.ThrowIfNull(trangThaiDuThao, "Không tìm thấy trạng thái 'Dự thảo'");
        var entity = new QuyetDinhDieuChinh
        {
            Id = Guid.NewGuid(),
            DuAnId = request.Dto.DuAnId,
            BuocId = request.Dto.BuocId,
            LoaiDieuChinhId = request.Dto.LoaiDieuChinhId,
            LyDo = request.Dto.LyDo,
            NgayQuyetDinh = request.Dto.NgayQuyetDinh,
            SoQuyetDinh = request.Dto.SoQuyetDinh,
            TrichYeu = request.Dto.TrichYeu,
            TrangThaiId = trangThaiDuThao?.Id??0,
            ThongTinDieuChinhChiPhi = request.Dto.ChiPhi == null
                ? null
                : new ThongTinDieuChinhChiPhi
                {
                    Id = Guid.NewGuid(),
                    TongMucDauTu = request.Dto.ChiPhi.TongMucDauTu,
                    ChiPhiXayLap = request.Dto.ChiPhi.ChiPhiXayLap,
                    ChiPhiThietBi = request.Dto.ChiPhi.ChiPhiThietBi,
                    ChiPhiKhac = request.Dto.ChiPhi.ChiPhiKhac,
                    ChiPhiDuPhong = request.Dto.ChiPhi.ChiPhiDuPhong
                }
        };

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);
       
        //if (request.Dto.ChiPhi is null)
        //{

        //    entity.ThongTinDieuChinhChiPhi ??= new ThongTinDieuChinhChiPhi();
        //}


        //await _unitOfWork.SaveChangesAsync(cancellationToken);
        //await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return entity;
    }
}