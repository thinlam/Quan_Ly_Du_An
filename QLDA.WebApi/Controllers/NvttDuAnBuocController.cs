using System.Net.Mime;
using QLDA.Application.DuAnBuocs.DTOs;
using QLDA.Application.DuAnBuocs.Queries;
using QLDA.Domain.Constants;

namespace QLDA.WebApi.Controllers;

/// <summary>
/// Dedicated NVTT-flavored read-only endpoint for the DuAnBuoc step tree.
/// Restricted to NVTT_BP01 / NVTT_XemDuAn roles — returns all steps for the
/// project regardless of ownership. Mirrors api/du-an-buoc/danh-sach/{duAnId}
/// 1:1 for payload shape. QLDA users should use the original endpoint
/// (which applies ownership filter).
/// </summary>
[Tags("NVTT - Dự án bước")]
[Route("api/nvtt/du-an-buoc")]
[Authorize(Roles = $"{RoleConstants.NVTT_BP01},{RoleConstants.NVTT_XemDuAn}")]
public class NvttDuAnBuocController(IServiceProvider serviceProvider) : AggregateRootController(serviceProvider)
{
    /// <summary>
    /// Danh sách bước (tree) theo dự án — không áp dụng ownership filter (NVTT xem tất cả).
    /// </summary>
    /// <remarks>
    /// Dùng cho show quy trình ở góc nhìn NVTT.
    /// </remarks>
    /// <param name="duAnId"></param>
    /// <returns></returns>
    [HttpGet("danh-sach/{duAnId}")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<ResultApi<List<DuAnBuocStateDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> GetDanhSach(Guid duAnId)
    {
        var res = await Mediator.Send(new DuAnBuocGetTreeListNvttQuery
        {
            DuAnId = duAnId,
        });
        return ResultApi.Ok(res);
    }
}
