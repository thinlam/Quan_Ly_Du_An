using System.Net.Mime;
using QLDA.Application.KySos.Commands;
using QLDA.Application.KySos.DTOs;
using QLDA.Application.KySos.Queries;
using QLDA.Application.TepDinhKems.Commands;
using QLDA.Domain.Constants;
using QLDA.WebApi.Models.KySos;
using QLDA.WebApi.Models.TepDinhKems;

namespace QLDA.WebApi.Controllers;

/// <summary>
/// Lưu file ký s06ac6528-df5a-f011-a9bf-0050568a8a95
/// </summary>
/// <param name="serviceProvider"></param>
[Tags("Ký số")]
[Route("api/ky-so")]
public class KySoController(IServiceProvider serviceProvider) : AggregateRootController(serviceProvider) {
    /// <summary>Danh sách file đã ký (TepDinhKem có ParentId).</summary>
    [HttpGet("noi-dung-da-ky/danh-sach")]
    [ProducesResponseType<ResultApi<PaginatedList<NoiDungDaKyDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> GetNoiDungDaKyList(
        [FromQuery] NoiDungDaKySearchDto searchDto,
        [FromQuery] AggregateRootPagination pagination) {
        var res = await Mediator.Send(new NoiDungDaKyGetDanhSachQuery(searchDto) {
            PageIndex = pagination.PageIndex,
            PageSize = pagination.PageSize,
        });
        return ResultApi.Ok(res);
    }

    /// <summary>
    ///
    /// </summary>
    /// <remarks>
    /// GroupId là id của dối tượng chính có file ký số - guid
    /// </remarks>
    /// <param name="model"></param>
    [ProducesResponseType<ResultApi<int>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpPost("them-moi")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Create([FromBody] KySoModel model) {
        ManagedException.ThrowIfNull(model.DanhSachTepDinhKem);
        model.DanhSachTepDinhKem ??= [];

        // ── Bước 1: Insert tệp đã ký vào TepDinhKem ──────────────────────────
        // DanhSachTepDinhKem chứa các tệp đã ký (ParentId != null)
        // Sau lệnh này: TepDinhKem mới với Id vừa được sinh
        await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand {
            KySo      = true,
            GroupId   = model.GroupId.ToString(),
            Entities  = [.. model.DanhSachTepDinhKem.ToEntities(model.GroupId, GroupTypeConstants.KySo)]
        });

        // ── Bước 2: Insert NoiDungDaKySo từ tệp đã ký mới ──────────────────────
        // Lấy TepDinhKem mới vừa được insert ở Bước 1 (ParentId != null)
        // Sau đó insert ID của tệp mới này vào NoiDungDaKySo
        var tepDaKyMoi = model.DanhSachTepDinhKem.FirstOrDefault(e => e.ParentId != null);
        if (tepDaKyMoi is not null) {
            await Mediator.Send(new NoiDungDaKyInsertCommand {
                TepDinhKemId = tepDaKyMoi.GetId(),     // ← ID tệp đã ký mới từ Bước 1
                FileName     = tepDaKyMoi.FileName,    // ← Tên tệp đã ký
                FileOrginal  = tepDaKyMoi.OriginalName, // ← Tên tệp gốc
                GroupId      = model.GroupId.ToString(),
                GroupName    = GroupTypeConstants.KySo,
            });
        }

        return ResultApi.Ok(1);
    }
}