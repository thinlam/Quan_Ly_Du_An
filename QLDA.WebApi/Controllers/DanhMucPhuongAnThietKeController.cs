using System.Net.Mime;
using QLDA.Domain.Constants;
using QLDA.WebApi.Models.DanhMucNhaThaus;

namespace QLDA.WebApi.Controllers {
    [Tags("Danh mục phương án thiết kế")]
    [Route("api/danh-muc-phuong-an-thiet-ke")]
    public class DanhMucPhuongAnThietKeController : AggregateRootController {
        public DanhMucPhuongAnThietKeController(IServiceProvider serviceProvider) : base(serviceProvider) {
        }

        [ProducesResponseType<ResultApi<DanhMucPhuongAnThietKe>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
        [HttpGet("{id}")]
        public async Task<ResultApi> Get(int id) {
            var entity = await Mediator.Send(new DanhMucGetQuery
                { Id = id.ToString(), DanhMuc = EDanhMuc.DanhMucPhuongAnThietKe, ThrowIfNull = true, }) as DanhMucPhuongAnThietKe;
            return ResultApi.Ok(entity);
        }


        /// <summary>
        /// Tạm ẩn
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ProducesResponseType<ResultApi<DanhMucPhuongAnThietKe>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = RoleConstants.GroupAdminOrManager)]
        [HttpDelete("xoa-tam")]
        public async Task<ResultApi> SoftDelete(int id) {
            var entity = await Mediator.Send(new DanhMucGetQuery
                { Id = id.ToString(), DanhMuc = EDanhMuc.DanhMucPhuongAnThietKe, ThrowIfNull = true, }) as DanhMucPhuongAnThietKe;
            entity!.IsDeleted = true;
            await Mediator.Send(new DanhMucInsertOrUpdateCommand(entity, EDanhMuc.DanhMucPhuongAnThietKe));
            return ResultApi.Ok(entity);
        }

        [ProducesResponseType<ResultApi<PaginatedList<DanhMucDto<int>>>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
        [HttpGet("danh-sach-day-du")]
        public async Task<ResultApi> GetAll([FromQuery] AggregateRootPagination req, string? globalFilter) {
            var res = await Mediator.Send(new DanhMucGetDanhSachQuery() {
                DanhMuc = EDanhMuc.DanhMucPhuongAnThietKe,
                PageIndex = req.PageIndex, GlobalFilter = globalFilter,
                PageSize = req.PageSize,
                GetAll = true
            });
            return ResultApi.Ok(res);
        }

       
        [ProducesResponseType<ResultApi<int>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = RoleConstants.GroupAdminOrManager)]
        [HttpPost("them-moi")]
        [Consumes(MediaTypeNames.Application.Json)]
        public async Task<ResultApi> Create([FromBody] DanhMucPhuongAnThietKe model) {
            await Mediator.Send(new DanhMucInsertOrUpdateCommand(model, EDanhMuc.DanhMucPhuongAnThietKe));
            return ResultApi.Ok(1);
        }

        [ProducesResponseType<ResultApi<DanhMucPhuongAnThietKe>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = RoleConstants.GroupAdminOrManager)]
        [HttpPut("cap-nhat")]
        [Consumes(MediaTypeNames.Application.Json)]
        public async Task<ResultApi> Update([FromBody] DanhMucPhuongAnThietKe model) {
            var entity = await Mediator.Send(new DanhMucGetQuery
                    { Id = model.Id.ToString(), DanhMuc = EDanhMuc.DanhMucPhuongAnThietKe, ThrowIfNull = true, }) as
                DanhMucPhuongAnThietKe;

            entity!.Ma = model.Ma;
            entity.Ten = model.Ten;
            entity.MoTa = model.MoTa;
            entity.Used = model.Used;

            await Mediator.Send(new DanhMucInsertOrUpdateCommand(entity, EDanhMuc.DanhMucPhuongAnThietKe));

            return ResultApi.Ok(model);
        }
    }
}