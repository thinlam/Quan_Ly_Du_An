using System.Net.Mime;
using QLDA.Application.DanhMucBuocs.Commands;
using QLDA.Application.DanhMucBuocs.DTOs;
using QLDA.Application.DanhMucBuocs.Queries;
using QLDA.Domain.Constants;

namespace QLDA.WebApi.Controllers {
    [Tags("Danh mục bước")]
    [Route("api/danh-muc-buoc")]
    public class DanhMucBuocController(IServiceProvider serviceProvider) : AggregateRootController(serviceProvider) {
        [HttpGet("{id}")]
        [ProducesResponseType<ResultApi>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
        public async Task<ResultApi> Get(int id) {
            var res = await Mediator.Send(new DanhMucBuocGetQuery() {
                Id = id,
                ThrowIfNull = true,
                IncludeScreen = true,
                IsNoTracking = true
            });

            return ResultApi.Ok(res.ToDto());
        }


        /// <summary>
        /// Tạm ẩn
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = RoleConstants.GroupAdminOrManager)]
        [HttpDelete("xoa-tam")]
        [ProducesResponseType<ResultApi<int>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
        public async Task<ResultApi> SoftDelete(int id) {
            var res = await Mediator.Send(new DanhMucBuocDeleteCommand(id));
            return ResultApi.Ok(res);
        }

        [HttpGet("danh-sach")]
        [ProducesResponseType<ResultApi>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
        public async Task<ResultApi> Get(
            [FromQuery] AggregateRootPagination pagination,
            [FromQuery] int? quyTrinhId = null,
            [FromQuery] int? giaiDoanId = null,
            [FromQuery] string? globalFilter = null,
            [FromQuery] List<long>? ids = null) {
            var res = await Mediator.Send(new DanhMucBuocGetDanhSachQuery() {
                GlobalFilter = globalFilter,
                QuyTrinhId = quyTrinhId,
                GiaiDoanId = giaiDoanId,
                PageIndex = pagination.PageIndex,
                PageSize = pagination.PageSize,
                Ids = ids,
            });
            return ResultApi.Ok(res.Data);
        }
        [HttpGet("combobox")]
        [ResponseCache(CacheProfileName = "Combobox")]
        [ProducesResponseType<ResultApi>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
        public async Task<ResultApi> GetCbo(int? quyTrinhId = null, int? giaiDoanId = null,
            string? globalFilter = null, [FromQuery] List<long>? ids = null) {
            var res = await Mediator.Send(new DanhMucBuocGetDanhSachQuery() {
                GlobalFilter = globalFilter,
                QuyTrinhId = quyTrinhId,
                GiaiDoanId = giaiDoanId,
                PageIndex = 0,
                PageSize = 0,
                Ids = ids,
                IsCbo = true
            });
            return ResultApi.Ok(res.Data);
        }

        [HttpGet("tree-list")]
        [ProducesResponseType<ResultApi>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
        public async Task<ResultApi> GetTreeList(int quyTrinhId) {
            var res = await Mediator.Send(new DanhMucBuocGetTreeListQuery() {
                QuyTrinhId = quyTrinhId,
            });
            return ResultApi.Ok(res);
        }

        [Authorize(Roles = RoleConstants.GroupAdminOrManager)]
        [HttpPost("them-moi")]
        [ProducesResponseType<ResultApi<DanhMucBuocDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
        [Consumes(MediaTypeNames.Application.Json)]
        public async Task<ResultApi> Create([FromBody] DanhMucBuocInsertDto insertDto) {
            var entity = await Mediator.Send(new DanhMucBuocInsertCommand(insertDto));
            return ResultApi.Ok(entity.ToDto());
        }

        [Authorize(Roles = RoleConstants.GroupAdminOrManager)]
        [HttpPut("cap-nhat")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType<ResultApi<DanhMucBuocDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
        public async Task<ResultApi> Update([FromBody] DanhMucBuocUpdateDto updateDto) {
            var entity = await Mediator.Send(new DanhMucBuocUpdateCommand(updateDto));
            return ResultApi.Ok(entity.ToDto());
        }

    }
}