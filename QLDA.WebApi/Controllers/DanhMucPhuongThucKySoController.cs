using System.Net.Mime;
using QLDA.Domain.Constants;
using QLDA.WebApi.Models.DanhMucPhuongThucKySos;

namespace QLDA.WebApi.Controllers {
    [Tags("Danh mục phương thức ký số")]
    [Route("api/danh-muc-phuong-thuc-ky-so")]
    [Authorize(Roles = RoleConstants.GroupAdminOrManager)]
    public class DanhMucPhuongThucKySoController(IServiceProvider serviceProvider)
        : AggregateRootController(serviceProvider) {
        [ProducesResponseType<ResultApi<DanhMucPhuongThucKySoModel>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
        [HttpGet("{id}")]
        public async Task<ResultApi> Get(int id) {
            var entity = await Mediator.Send(new DanhMucGetQuery() {
                DanhMuc = EDanhMuc.DanhMucPhuongThucKySo,
                Id = id.ToString(),
                ThrowIfNull = true,
            }) as DanhMucPhuongThucKySo;
            var model = entity!.ToModel();

            return ResultApi.Ok(model);
        }


        /// <summary>
        /// Tạm ẩn
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ProducesResponseType<ResultApi<DanhMucPhuongThucKySo>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
        [HttpDelete("xoa-tam")]
        public async Task<ResultApi> SoftDelete(int id) {
            var entity =
                await Mediator.Send(new DanhMucGetQuery() {
                    Id = id.ToString(),
                    DanhMuc = EDanhMuc.DanhMucPhuongThucKySo,
                    ThrowIfNull = true
                }) as DanhMucPhuongThucKySo;
            entity!.IsDeleted = true;
            await Mediator.Send(new DanhMucInsertOrUpdateCommand(entity, EDanhMuc.DanhMucPhuongThucKySo));
            return ResultApi.Ok(entity);
        }

        [ProducesResponseType<ResultApi<PaginatedList<DanhMucDto<int>>>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
        [HttpGet("danh-sach-day-du")]
        public async Task<ResultApi> GetAll([FromQuery] AggregateRootPagination req, string? globalFilter) {
            var res = await Mediator.Send(new DanhMucGetDanhSachQuery() {
                DanhMuc = EDanhMuc.DanhMucPhuongThucKySo,
                GlobalFilter = globalFilter,
                PageIndex = req.PageIndex,
                PageSize = req.PageSize,
                GetAll = true
            });
            return ResultApi.Ok(res);
        }

        [ProducesResponseType<ResultApi<List<DanhMucDto<int>>>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
        [HttpGet("danh-sach")]
        public async Task<ResultApi> Get(string? globalFilter = null, [FromQuery] List<long>? ids = null,
            bool getAll = false) {
            var res = await Mediator.Send(new DanhMucGetDanhSachQuery() {
                GlobalFilter = globalFilter,
                DanhMuc = EDanhMuc.DanhMucPhuongThucKySo,
                PageIndex = 0,
                PageSize = 0,
                Ids = ids,
                GetAll = getAll,
            }) as PaginatedList<DanhMucDto<int>>;
            return ResultApi.Ok(res == null ? [] : res.Data);
        }

        [ProducesResponseType<ResultApi<int>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
        [HttpPost("them-moi")]
        [Consumes(MediaTypeNames.Application.Json)]
        public async Task<ResultApi> Create([FromBody] DanhMucPhuongThucKySoModel model) {
            var entity = model.ToEntity();
            await Mediator.Send(new DanhMucInsertOrUpdateCommand(entity, EDanhMuc.DanhMucPhuongThucKySo));
            return ResultApi.Ok(1);
        }

        [ProducesResponseType<ResultApi<DanhMucPhuongThucKySoModel>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
        [HttpPut("cap-nhat")]
        [Consumes(MediaTypeNames.Application.Json)]
        public async Task<ResultApi> Update([FromBody] DanhMucPhuongThucKySoModel model) {
            var entity = await Mediator.Send(new DanhMucGetQuery() {
                Id = model.GetId().ToString(),
                ThrowIfNull = true,
                DanhMuc = EDanhMuc.DanhMucPhuongThucKySo
            }) as DanhMucPhuongThucKySo;

            entity!.Ma = model.Ma;
            entity.Ten = model.Ten;
            entity.MoTa = model.MoTa;
            entity.Stt = model.Stt;
            entity.Used = model.Used;

            await Mediator.Send(new DanhMucInsertOrUpdateCommand(entity, EDanhMuc.DanhMucPhuongThucKySo));
            return ResultApi.Ok(model);
        }
    }
}