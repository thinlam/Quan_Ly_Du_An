using System.Net.Mime;
using QLDA.Application.QuanLyPheDuyet.DTOs;
using QLDA.Application.QuanLyPheDuyet.Queries;
using QLDA.Application.QuyetDinhDieuChinhs.Commands;
using QLDA.Application.QuyetDinhDieuChinhs.Queries;
using QLDA.Domain.Constants;

namespace QLDA.WebApi.Controllers;

/// <summary>
/// Quyết định điều chỉnh phê duyệt (UC64 - #9483)
/// </summary>
[Tags("Quyết định điều chỉnh")]
[Route("api/quyet-dinh-dieu-chinh")]
public class QuyetDinhDieuChinhController : AggregateRootController {
    public QuyetDinhDieuChinhController(IServiceProvider serviceProvider) : base(serviceProvider) { }

    /// <summary>
    /// Danh sách quyết định điều chỉnh
    /// </summary>
    [ProducesResponseType(typeof(ResultApi<PagedResultDto<QuyetDinhDieuChinhListItemDto>>), StatusCodes.Status200OK)]
    [HttpGet("danh-sach")]
    public async Task<ResultApi> GetDanhSach(
        [FromQuery] Guid? duAnId,
        [FromQuery] string? pheDuyetEntityName,
        [FromQuery] Guid? pheDuyetEntityId,
        [FromQuery] string? globalFilter = null,
        int page = 1,
        int pageSize = 20) {
        var result = await Mediator.Send(new QuyetDinhDieuChinhGetDanhSachQuery {
            DuAnId = duAnId,
            PheDuyetEntityName = pheDuyetEntityName,
            PheDuyetEntityId = pheDuyetEntityId,
            GlobalFilter = globalFilter,
            Page = page,
            PageSize = pageSize
        });
        return ResultApi.Ok(result);
    }

    /// <summary>
    /// Chi tiết quyết định điều chỉnh
    /// </summary>
    [ProducesResponseType(typeof(ResultApi<QuyetDinhDieuChinhChiTietDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultApi), StatusCodes.Status400BadRequest)]
    [HttpGet("{id}/chi-tiet")]
    public async Task<ResultApi> GetChiTiet(Guid id) {
        var result = await Mediator.Send(new QuyetDinhDieuChinhGetChiTietQuery(id));
        return ResultApi.Ok(result);
    }

    /// <summary>
    /// Lịch sử xử lý quyết định điều chỉnh
    /// </summary>
    [ProducesResponseType(typeof(ResultApi<PaginatedList<PheDuyetHistoryDto>>), StatusCodes.Status200OK)]
    [HttpGet("{id}/lich-su")]
    public async Task<ResultApi> GetLichSu(Guid id) {
        var result = await Mediator.Send(new PheDuyetGetLichSuQuery {
            Type = PheDuyetEntityNames.QuyetDinhDieuChinh,
            EntityId = id
        });
        return ResultApi.Ok(result);
    }

    /// <summary>
    /// Thêm mới quyết định điều chỉnh
    /// </summary>
    [ProducesResponseType(typeof(ResultApi<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultApi), StatusCodes.Status400BadRequest)]
    [HttpPost("them-moi")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Create([FromBody] QuyetDinhDieuChinhModel model, CancellationToken cancellationToken) {
        var dto = new QuyetDinhDieuChinhDto {
            PheDuyetEntityId = model.PheDuyetEntityId,
            PheDuyetEntityName = model.PheDuyetEntityName,
            DuAnId = model.DuAnId,
            SoQuyetDinh = model.SoQuyetDinh,
            NgayQuyetDinh = model.NgayQuyetDinh,
            TrichYeu = model.TrichYeu,
            LoaiDieuChinhId = model.LoaiDieuChinhId,
            LyDo = model.LyDo,
            TepDinhKem = model.TepDinhKem,
            ChiPhis = model.ChiPhis?.Select(c => new ThongTinDieuChinhChiPhiDto {
                TongMucDauTu = c.TongMucDauTu,
                ChiPhiXayLap = c.ChiPhiXayLap,
                ChiPhiThietBi = c.ChiPhiThietBi,
                ChiPhiKhac = c.ChiPhiKhac,
                ChiPhiDuPhong = c.ChiPhiDuPhong
            }).ToList()
        };

        var result = await Mediator.Send(new QuyetDinhDieuChinhInsertCommand(dto), cancellationToken);
        return ResultApi.Ok(result);
    }

    /// <summary>
    /// Cập nhật quyết định điều chỉnh
    /// </summary>
    [ProducesResponseType(typeof(ResultApi<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultApi), StatusCodes.Status400BadRequest)]
    [HttpPut("cap-nhat")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Update([FromBody] QuyetDinhDieuChinhModel model, CancellationToken cancellationToken) {
        var dto = new QuyetDinhDieuChinhDto {
            PheDuyetEntityId = model.PheDuyetEntityId,
            PheDuyetEntityName = model.PheDuyetEntityName,
            DuAnId = model.DuAnId,
            SoQuyetDinh = model.SoQuyetDinh,
            NgayQuyetDinh = model.NgayQuyetDinh,
            TrichYeu = model.TrichYeu,
            LoaiDieuChinhId = model.LoaiDieuChinhId,
            LyDo = model.LyDo,
            TepDinhKem = model.TepDinhKem,
            ChiPhis = model.ChiPhis?.Select(c => new ThongTinDieuChinhChiPhiDto {
                TongMucDauTu = c.TongMucDauTu,
                ChiPhiXayLap = c.ChiPhiXayLap,
                ChiPhiThietBi = c.ChiPhiThietBi,
                ChiPhiKhac = c.ChiPhiKhac,
                ChiPhiDuPhong = c.ChiPhiDuPhong
            }).ToList()
        };

        var result = await Mediator.Send(new QuyetDinhDieuChinhUpdateCommand(dto), cancellationToken);
        return ResultApi.Ok(result);
    }

    /// <summary>
    /// Xóa quyết định điều chỉnh
    /// </summary>
    [ProducesResponseType(typeof(ResultApi<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultApi), StatusCodes.Status400BadRequest)]
    [HttpDelete("{id}/xoa")]
    public async Task<ResultApi> Delete(Guid id, CancellationToken cancellationToken) {
        var result = await Mediator.Send(new QuyetDinhDieuChinhDeleteCommand(id), cancellationToken);
        return ResultApi.Ok(result);
    }

    /// <summary>
    /// Trình điều chỉnh
    /// </summary>
    [ProducesResponseType(typeof(ResultApi<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultApi), StatusCodes.Status400BadRequest)]
    [HttpPost("{id}/trinh")]
    public async Task<ResultApi> Trinh(Guid id, CancellationToken cancellationToken) {
        var result = await Mediator.Send(new QuyetDinhDieuChinhTrinhCommand(id), cancellationToken);
        return ResultApi.Ok(result);
    }

    /// <summary>
    /// Thẩm định điều chỉnh
    /// </summary>
    [ProducesResponseType(typeof(ResultApi<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultApi), StatusCodes.Status400BadRequest)]
    [HttpPost("{id}/tham-dinh")]
    public async Task<ResultApi> ThamDinh(Guid id, CancellationToken cancellationToken) {
        var result = await Mediator.Send(new QuyetDinhDieuChinhThamDinhCommand(id), cancellationToken);
        return ResultApi.Ok(result);
    }

    /// <summary>
    /// Trình phê duyệt điều chỉnh
    /// </summary>
    [ProducesResponseType(typeof(ResultApi<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultApi), StatusCodes.Status400BadRequest)]
    [HttpPost("{id}/trinh-phe-duyet")]
    public async Task<ResultApi> TrinhPheDuyet(Guid id, CancellationToken cancellationToken) {
        var result = await Mediator.Send(new QuyetDinhDieuChinhTrinhPheDuyetCommand(id), cancellationToken);
        return ResultApi.Ok(result);
    }

    /// <summary>
    /// Duyệt điều chỉnh
    /// </summary>
    [ProducesResponseType(typeof(ResultApi<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultApi), StatusCodes.Status400BadRequest)]
    [HttpPost("{id}/duyet")]
    public async Task<ResultApi> Duyet(Guid id, CancellationToken cancellationToken) {
        var result = await Mediator.Send(new QuyetDinhDieuChinhDuyetCommand(id), cancellationToken);
        return ResultApi.Ok(result);
    }

    /// <summary>
    /// Trả lại điều chỉnh
    /// </summary>
    [ProducesResponseType(typeof(ResultApi<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultApi), StatusCodes.Status400BadRequest)]
    [HttpPost("{id}/tra-lai")]
    public async Task<ResultApi> TraLai(Guid id, [FromBody] LyDoModel model, CancellationToken cancellationToken) {
        var result = await Mediator.Send(new QuyetDinhDieuChinhTraLaiCommand(id, model.NoiDung ?? ""), cancellationToken);
        return ResultApi.Ok(result);
    }

    /// <summary>
    /// Từ chối điều chỉnh
    /// </summary>
    [ProducesResponseType(typeof(ResultApi<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultApi), StatusCodes.Status400BadRequest)]
    [HttpPost("{id}/tu-choi")]
    public async Task<ResultApi> TuChoi(Guid id, [FromBody] LyDoModel model, CancellationToken cancellationToken) {
        var result = await Mediator.Send(new QuyetDinhDieuChinhTuChoiCommand(id, model.NoiDung ?? ""), cancellationToken);
        return ResultApi.Ok(result);
    }
}

public class QuyetDinhDieuChinhModel {
    public Guid PheDuyetEntityId { get; set; }
    public string PheDuyetEntityName { get; set; } = default!;
    public Guid DuAnId { get; set; }
    public string? SoQuyetDinh { get; set; }
    public DateTimeOffset? NgayQuyetDinh { get; set; }
    public string? TrichYeu { get; set; }
    public int LoaiDieuChinhId { get; set; }
    public string? LyDo { get; set; }
    public string? TepDinhKem { get; set; }
    public List<ThongTinDieuChinhChiPhiModel>? ChiPhis { get; set; }
}

public class ThongTinDieuChinhChiPhiModel {
    public decimal? TongMucDauTu { get; set; }
    public decimal? ChiPhiXayLap { get; set; }
    public decimal? ChiPhiThietBi { get; set; }
    public decimal? ChiPhiKhac { get; set; }
    public decimal? ChiPhiDuPhong { get; set; }
}

public class LyDoModel {
    public string? NoiDung { get; set; }
}