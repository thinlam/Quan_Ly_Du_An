using System.Net.Mime;
using QLDA.Application.DuAns.DTOs;
using QLDA.Application.DuAns.Queries;
using QLDA.Domain.Constants;

namespace QLDA.WebApi.Controllers;

/// <summary>
/// Dedicated NVTT-flavored read-only endpoint for the DuAn list. Restricted
/// to NVTT_BP01 / NVTT_XemDuAn roles — returns all DuAn across the system
/// (no ownership filter). Mirrors api/du-an/danh-sach 1:1 for payload shape.
/// QLDA users should use api/du-an/danh-sach (with ownership filter).
/// </summary>
[Tags("NVTT - Dự án")]
[Route("api/nvtt/du-an")]
[Authorize(Roles = $"{RoleConstants.NVTT_BP01},{RoleConstants.NVTT_XemDuAn}")]
public class NvttDuAnController(IServiceProvider serviceProvider) : AggregateRootController(serviceProvider)
{
    /// <summary>
    /// Danh sách phân trang dự án — không áp dụng ownership filter (NVTT xem tất cả).
    /// </summary>
    /// <param name="searchDto"></param>
    /// <returns></returns>
    [HttpGet("danh-sach")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<ResultApi<PaginatedList<DuAnDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> Get([FromQuery] DuAnSearchDto searchDto)
    {
        var res = await Mediator.Send(new DuAnGetDanhSachNvttQuery(searchDto));
        return ResultApi.Ok(res);
    }
}
