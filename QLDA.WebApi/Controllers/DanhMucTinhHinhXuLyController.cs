using System.Net.Mime;
using QLDA.Domain.Constants;
using QLDA.WebApi.Models.DanhMucTinhHinhXuLys;

namespace QLDA.WebApi.Controllers {
    [Tags("Danh mục tình hình xử lý")]
    [Route("api/danh-muc-tinh-hinh-xu-ly")]
    [Authorize(Roles = RoleConstants.GroupAdminOrManager)]
    public class DanhMucTinhHinhXuLyController(IServiceProvider serviceProvider)
        : AggregateRootController(serviceProvider) {
        [ProducesResponseType<ResultApi<DanhMucTinhHinhXuLy>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
        [HttpGet("{id}")]
        public async Task<ResultApi> Get(int id) {
            var entity = await Mediator.Send(new DanhMucGetQuery{Id = id.ToString(), DanhMuc = EDanhMuc.DanhMucTinhHinhXuLy,ThrowIfNull = true, }) as DanhMucTinhHinhXuLy;
            var model = entity!.ToModel();
            return ResultApi.Ok(model);
        }
        /// <summary>
        /// Tạm ẩn
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ProducesResponseType<ResultApi<DanhMucTinhHinhXuLy>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
        [HttpDelete("xoa-tam")]
        public async Task<ResultApi> SoftDelete(int id) {
            var entity = await Mediator.Send(new DanhMucGetQuery{Id = id.ToString(), DanhMuc = EDanhMuc.DanhMucTinhHinhXuLy, ThrowIfNull = true, }) as DanhMucTinhHinhXuLy;
            entity!.IsDeleted = true;
            await Mediator.Send(new DanhMucInsertOrUpdateCommand(entity, EDanhMuc.DanhMucTinhHinhXuLy));
            return ResultApi.Ok(entity);
        }

        [ProducesResponseType<ResultApi<PaginatedList<DanhMucDto<int>>>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
        [HttpGet("danh-sach-day-du")]
        public async Task<ResultApi> GetAll([FromQuery] AggregateRootPagination req, string? globalFilter) {
            var res = await Mediator.Send(new DanhMucGetDanhSachQuery() {
                DanhMuc = EDanhMuc.DanhMucTinhHinhXuLy,
                PageIndex = req.PageIndex, GlobalFilter = globalFilter,
                PageSize = req.PageSize,
                GetAll = true
            });
            return ResultApi.Ok(res);
        }

        [ProducesResponseType<ResultApi<List<DanhMucDto<int>>>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
        [HttpGet("danh-sach")]
        public async Task<ResultApi> Get([FromQuery] List<long>? ids = null, bool getAll = false) {
            var res = await Mediator.Send(new DanhMucGetDanhSachQuery() {
                DanhMuc = EDanhMuc.DanhMucTinhHinhXuLy,
                PageIndex = 0,
                PageSize = 0,
                Ids = ids, GetAll = getAll,
            }) as PaginatedList<DanhMucDto<int>>;
            return ResultApi.Ok(res == null ? [] :res.Data);
        }


        [ProducesResponseType<ResultApi<int>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
        [HttpPost("them-moi")]
        [Consumes(MediaTypeNames.Application.Json)]
        public async Task<ResultApi> Create([FromBody] DanhMucTinhHinhXuLyModel model) {
            var entity = model.ToEntity();
            await Mediator.Send(new DanhMucInsertOrUpdateCommand(entity, EDanhMuc.DanhMucTinhHinhXuLy));
            return ResultApi.Ok(1);
        }

        [ProducesResponseType<ResultApi<DanhMucTinhHinhXuLyModel>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
        [HttpPut("cap-nhat")]
        [Consumes(MediaTypeNames.Application.Json)]
        public async Task<ResultApi> Update([FromBody] DanhMucTinhHinhXuLyModel model) {
            var entity = await Mediator.Send(new DanhMucGetQuery{Id = model.GetId().ToString(), DanhMuc = EDanhMuc.DanhMucTinhHinhXuLy, ThrowIfNull = true, }) as DanhMucTinhHinhXuLy;

            entity!.Ma = model.Ma;
            entity.Ten = model.Ten;
            entity.MoTa = model.MoTa;
            entity.Stt = model.Stt;entity.Used = model.Used;

            await Mediator.Send(new DanhMucInsertOrUpdateCommand(entity, EDanhMuc.DanhMucTinhHinhXuLy));

            return ResultApi.Ok(model);
        }
    }
}