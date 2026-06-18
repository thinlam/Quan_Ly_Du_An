using QLDA.Application.DanhMucDonVis.Queries;
using QLDA.Domain.Constants;

namespace QLDA.WebApi.Controllers
{
    [Tags("Danh mục đơn vị")]
    [Route("api/danh-muc-don-vi")]
    [Authorize(Roles = RoleConstants.GroupAdminOrManager)]
    public class DanhMucDonViController(IServiceProvider serviceProvider) : AggregateRootController(serviceProvider)
    {
        /// <summary>
        /// Danh mục đơn vị lấy trực tiếp từ VI_MASTER
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType<ResultApi>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
        [HttpGet("danh-sach")]
        public async Task<ResultApi> Get([FromQuery] Guid? DuAnId = null)
        {
            var res = await Mediator.Send(new DanhMucDonViGetDanhSachQuery()
            {
                PageIndex = 0,
                PageSize = 0,
                CapDonViIds = [4, 5],
                DuAnId = DuAnId,
            });
            return ResultApi.Ok(res.Data);
        }
    }
}