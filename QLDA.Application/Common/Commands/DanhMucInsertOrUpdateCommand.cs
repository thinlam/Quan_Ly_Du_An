using QLDA.Application.Common.Enums;

namespace QLDA.Application.Common.Commands;

public record DanhMucInsertOrUpdateCommand(object Entity, EDanhMuc DanhMuc, bool IsEnum = false) : IRequest {
}

internal class DanhMucInsertOrUpdateCommandHandler : IRequestHandler<DanhMucInsertOrUpdateCommand> {
    private readonly ICrudService<DanhMucBuoc, int> DanhMucBuoc;
    private readonly ICrudService<DanhMucBuocTrangThaiTienDo, int> DanhMucBuocTrangThaiTienDo;
    private readonly ICrudService<DanhMucChuDauTu, int> DanhMucChuDauTu;
    private readonly ICrudService<DanhMucHinhThucDauTu, int> DanhMucHinhThucDauTu;
    private readonly ICrudService<DanhMucHinhThucQuanLy, int> DanhMucHinhThucQuanLy;
    private readonly ICrudService<DanhMucLinhVuc, int> DanhMucLinhVuc;
    private readonly ICrudService<DanhMucLoaiDuAn, int> DanhMucLoaiDuAn;
    private readonly ICrudService<DanhMucLoaiDuAnTheoNam, int> DanhMucLoaiDuAnTheoNam;
    private readonly ICrudService<DanhMucNguonVon, int> DanhMucNguonVon;
    private readonly ICrudService<DanhMucNhomDuAn, int> DanhMucNhomDuAn;
    private readonly IRepository<DanhMucQuyTrinh, int> DanhMucQuyTrinh;
    private readonly ICrudService<DanhMucTrangThaiDuAn, int> DanhMucTrangThaiDuAn;
    private readonly ICrudService<DanhMucTrangThaiTienDo, int> DanhMucTrangThaiTienDo;
    private readonly ICrudService<DanhMucManHinh, int> DanhMucManHinh;
    private readonly ICrudService<DanhMucChucVu, int> DanhMucChucVu;
    private readonly ICrudService<DanhMucLoaiVanBan, int> DanhMucLoaiVanBan;
    private readonly ICrudService<DanhMucLoaiGoiThau, int> DanhMucLoaiGoiThau;
    private readonly ICrudService<DanhMucHinhThucLuaChonNhaThau, int> DanhMucHinhThucLuaChonNhaThau;
    private readonly ICrudService<DanhMucPhuongThucLuaChonNhaThau, int> DanhMucPhuongThucLuaChonNhaThau;
    private readonly ICrudService<DanhMucLoaiHopDong, int> DanhMucLoaiHopDong;
    private readonly ICrudService<DanhMucTinhTrangKhoKhan, int> DanhMucTinhTrangKhoKhan;
    private readonly ICrudService<DanhMucGiaiDoan, int> DanhMucGiaiDoan;
    private readonly ICrudService<DanhMucPhuongThucKySo, int> DanhMucPhuongThucKySo;
    private readonly ICrudService<DanhMucNhaThau, Guid> DanhMucNhaThau;
    private readonly ICrudService<DanhMucMucDoKhoKhan, int> DanhMucMucDoKhoKhan;
    private readonly ICrudService<DanhMucTinhTrangThucHienLcnt, int> DanhMucTinhTrangThucHienLcnt;
    private readonly IUnitOfWork _unitOfWork;

    public DanhMucInsertOrUpdateCommandHandler(IServiceProvider serviceProvider) {
        DanhMucBuoc = serviceProvider.GetRequiredService<ICrudService<DanhMucBuoc, int>>();
        DanhMucBuocTrangThaiTienDo = serviceProvider.GetRequiredService<ICrudService<DanhMucBuocTrangThaiTienDo, int>>();
        DanhMucChuDauTu = serviceProvider.GetRequiredService<ICrudService<DanhMucChuDauTu, int>>();
        DanhMucHinhThucDauTu = serviceProvider.GetRequiredService<ICrudService<DanhMucHinhThucDauTu, int>>();
        DanhMucHinhThucQuanLy = serviceProvider.GetRequiredService<ICrudService<DanhMucHinhThucQuanLy, int>>();
        DanhMucLinhVuc = serviceProvider.GetRequiredService<ICrudService<DanhMucLinhVuc, int>>();
        DanhMucLoaiDuAn = serviceProvider.GetRequiredService<ICrudService<DanhMucLoaiDuAn, int>>();
        DanhMucLoaiDuAnTheoNam = serviceProvider.GetRequiredService<ICrudService<DanhMucLoaiDuAnTheoNam, int>>();
        DanhMucNguonVon = serviceProvider.GetRequiredService<ICrudService<DanhMucNguonVon, int>>();
        DanhMucNhomDuAn = serviceProvider.GetRequiredService<ICrudService<DanhMucNhomDuAn, int>>();
        DanhMucQuyTrinh = serviceProvider.GetRequiredService<IRepository<DanhMucQuyTrinh, int>>();
        DanhMucTrangThaiDuAn = serviceProvider.GetRequiredService<ICrudService<DanhMucTrangThaiDuAn, int>>();
        DanhMucTrangThaiTienDo = serviceProvider.GetRequiredService<ICrudService<DanhMucTrangThaiTienDo, int>>();
        DanhMucManHinh = serviceProvider.GetRequiredService<ICrudService<DanhMucManHinh, int>>();
        DanhMucChucVu = serviceProvider.GetRequiredService<ICrudService<DanhMucChucVu, int>>();
        DanhMucLoaiVanBan = serviceProvider.GetRequiredService<ICrudService<DanhMucLoaiVanBan, int>>();
        DanhMucLoaiGoiThau = serviceProvider.GetRequiredService<ICrudService<DanhMucLoaiGoiThau, int>>();
        DanhMucHinhThucLuaChonNhaThau = serviceProvider.GetRequiredService<ICrudService<DanhMucHinhThucLuaChonNhaThau, int>>();
        DanhMucPhuongThucLuaChonNhaThau = serviceProvider.GetRequiredService<ICrudService<DanhMucPhuongThucLuaChonNhaThau, int>>();
        DanhMucLoaiHopDong = serviceProvider.GetRequiredService<ICrudService<DanhMucLoaiHopDong, int>>();
        DanhMucTinhTrangKhoKhan = serviceProvider.GetRequiredService<ICrudService<DanhMucTinhTrangKhoKhan, int>>();
        DanhMucGiaiDoan = serviceProvider.GetRequiredService<ICrudService<DanhMucGiaiDoan, int>>();
        DanhMucPhuongThucKySo = serviceProvider.GetRequiredService<ICrudService<DanhMucPhuongThucKySo, int>>();
        DanhMucNhaThau = serviceProvider.GetRequiredService<ICrudService<DanhMucNhaThau, Guid>>();
        DanhMucMucDoKhoKhan = serviceProvider.GetRequiredService<ICrudService<DanhMucMucDoKhoKhan, int>>();
        DanhMucTinhTrangThucHienLcnt = serviceProvider.GetRequiredService<ICrudService<DanhMucTinhTrangThucHienLcnt, int>>();
        _unitOfWork = DanhMucQuyTrinh.UnitOfWork;
    }

    public async Task Handle(DanhMucInsertOrUpdateCommand request, CancellationToken cancellationToken) {
        switch (request.DanhMuc) {
            case EDanhMuc.DanhMucBuoc: {
                    await DanhMucBuoc.AddOrUpdateAsync((DanhMucBuoc)request.Entity,
                        cancellationToken: cancellationToken);
                    break;
                }
            case EDanhMuc.DanhMucBuocTrangThaiTienDo: {
                    await DanhMucBuocTrangThaiTienDo.AddOrUpdateAsync((DanhMucBuocTrangThaiTienDo)request.Entity,
                        cancellationToken: cancellationToken);
                    break;
                }
            case EDanhMuc.DanhMucChuDauTu: {
                    await DanhMucChuDauTu.AddOrUpdateAsync((DanhMucChuDauTu)request.Entity,
                        cancellationToken: cancellationToken);
                    break;
                }
            case EDanhMuc.DanhMucHinhThucDauTu: {
                    await DanhMucHinhThucDauTu.AddOrUpdateAsync((DanhMucHinhThucDauTu)request.Entity,
                        cancellationToken: cancellationToken);
                    break;
                }
            case EDanhMuc.DanhMucHinhThucQuanLy: {
                    await DanhMucHinhThucQuanLy.AddOrUpdateAsync((DanhMucHinhThucQuanLy)request.Entity,
                        cancellationToken: cancellationToken);
                    break;
                }
            case EDanhMuc.DanhMucLinhVuc: {
                    await DanhMucLinhVuc.AddOrUpdateAsync((DanhMucLinhVuc)request.Entity,
                        cancellationToken: cancellationToken);
                    break;
                }
            case EDanhMuc.DanhMucLoaiDuAn: {
                    await DanhMucLoaiDuAn.AddOrUpdateAsync((DanhMucLoaiDuAn)request.Entity,
                        cancellationToken: cancellationToken);
                    break;
                }
            case EDanhMuc.DanhMucLoaiDuAnTheoNam: {
                    await DanhMucLoaiDuAnTheoNam.AddOrUpdateAsync((DanhMucLoaiDuAnTheoNam)request.Entity,
                        cancellationToken: cancellationToken);
                    break;
                }
            case EDanhMuc.DanhMucNguonVon: {
                    await DanhMucNguonVon.AddOrUpdateAsync((DanhMucNguonVon)request.Entity,
                        cancellationToken: cancellationToken);
                    break;
                }
            case EDanhMuc.DanhMucNhomDuAn: {
                    await DanhMucNhomDuAn.AddOrUpdateAsync((DanhMucNhomDuAn)request.Entity,
                        cancellationToken: cancellationToken);
                    break;
                }
            case EDanhMuc.DanhMucQuyTrinh: {
                    var entity = (DanhMucQuyTrinh)request.Entity;

                    //Kiểm tra xem đã có quy trình mặc định nào khác chưa

                    if (DanhMucQuyTrinh.GetQueryableSet().Any(e => e.Id != entity.Id && e.MacDinh))
                        entity.MacDinh = false;

                    await DanhMucQuyTrinh.AddOrUpdateAsync(entity, cancellationToken: cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    break;
                }
            case EDanhMuc.DanhMucTrangThaiTienDo: {
                    await DanhMucTrangThaiTienDo.AddOrUpdateAsync((DanhMucTrangThaiTienDo)request.Entity,
                        cancellationToken: cancellationToken);
                    break;
                }
            case EDanhMuc.DanhMucTrangThaiDuAn: {
                    await DanhMucTrangThaiDuAn.AddOrUpdateAsync((DanhMucTrangThaiDuAn)request.Entity,
                        cancellationToken: cancellationToken);
                    break;
                }
            case EDanhMuc.DanhMucManHinh: {
                    await DanhMucManHinh.AddOrUpdateAsync((DanhMucManHinh)request.Entity, request.IsEnum,
                                    cancellationToken: cancellationToken);
                    break;
                }
            case EDanhMuc.DanhMucChucVu: {
                    await DanhMucChucVu.AddOrUpdateAsync((DanhMucChucVu)request.Entity,
                        cancellationToken: cancellationToken);
                    break;
                }
            case EDanhMuc.DanhMucLoaiVanBan: {
                    await DanhMucLoaiVanBan.AddOrUpdateAsync((DanhMucLoaiVanBan)request.Entity,
                        cancellationToken: cancellationToken);
                    break;
                }
            case EDanhMuc.DanhMucLoaiGoiThau: {
                    await DanhMucLoaiGoiThau.AddOrUpdateAsync((DanhMucLoaiGoiThau)request.Entity,
                        cancellationToken: cancellationToken);
                    break;
                }
            case EDanhMuc.DanhMucHinhThucLuaChonNhaThau: {
                    await DanhMucHinhThucLuaChonNhaThau.AddOrUpdateAsync((DanhMucHinhThucLuaChonNhaThau)request.Entity,
                        cancellationToken: cancellationToken);
                    break;
                }
            case EDanhMuc.DanhMucPhuongThucLuaChonNhaThau: {
                    await DanhMucPhuongThucLuaChonNhaThau.AddOrUpdateAsync((DanhMucPhuongThucLuaChonNhaThau)request.Entity,
                        cancellationToken: cancellationToken);
                    break;
                }
            case EDanhMuc.DanhMucLoaiHopDong: {
                    await DanhMucLoaiHopDong.AddOrUpdateAsync((DanhMucLoaiHopDong)request.Entity,
                        cancellationToken: cancellationToken);
                    break;
                }
            case EDanhMuc.DanhMucTinhTrangKhoKhan: {
                    await DanhMucTinhTrangKhoKhan.AddOrUpdateAsync((DanhMucTinhTrangKhoKhan)request.Entity,
                        cancellationToken: cancellationToken);
                    break;
                }
            case EDanhMuc.DanhMucGiaiDoan: {
                    await DanhMucGiaiDoan.AddOrUpdateAsync((DanhMucGiaiDoan)request.Entity,
                        cancellationToken: cancellationToken);
                    break;
                }
            case EDanhMuc.DanhMucPhuongThucKySo: {
                    await DanhMucPhuongThucKySo.AddOrUpdateAsync((DanhMucPhuongThucKySo)request.Entity,
                        cancellationToken: cancellationToken);
                    break;
                }
            case EDanhMuc.DanhMucNhaThau: {
                    await DanhMucNhaThau.AddOrUpdateAsync((DanhMucNhaThau)request.Entity,
                        cancellationToken: cancellationToken);
                    break;
                }
            case EDanhMuc.DanhMucMucDoKhoKhan: {
                    await DanhMucMucDoKhoKhan.AddOrUpdateAsync((DanhMucMucDoKhoKhan)request.Entity,
                        cancellationToken: cancellationToken);
                    break;
                }
            case EDanhMuc.DanhMucTinhTrangThucHienLcnt: {
                    await DanhMucTinhTrangThucHienLcnt.AddOrUpdateAsync((DanhMucTinhTrangThucHienLcnt)request.Entity,
                        cancellationToken: cancellationToken);
                    break;
                }
            default:
                break;
        }
    }
}