using QLDA.Application.UserMasters.Queries;
using QLDA.Domain.Constants;

namespace QLDA.WebApi.Controllers {
    [Tags("Danh mục người dùng")]
    [Route("api/danh-muc-user")]
    public class DanhMucUserController : AggregateRootController {
        public DanhMucUserController(IServiceProvider serviceProvider) : base(serviceProvider) {
        }

        [ProducesResponseType<ResultApi>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
        [HttpGet("danh-sach")]
        public async Task<ResultApi> Get([FromQuery] List<long>? ids = null, bool getAll = false) {
            var res = await Mediator.Send(new UserMasterGetDanhSachQuery() {
                PageIndex = 0,
                PageSize = 0,
                Ids = ids,
                GetAll = getAll,
            });
            return ResultApi.Ok(res.Data);
        }
        [ProducesResponseType<ResultApi>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
        [ResponseCache(CacheProfileName = "Combobox")]
        [HttpGet("combobox/lanh-dao")]
        public async Task<ResultApi> GetLeaders() {
            var data = await Mediator.Send(new GetUserByRoleNameQuery($"{RoleConstants.QLDA_LDDV}", DonViID: null, PhongBanID: null));
            return ResultApi.Ok(data);
        }
    }
}