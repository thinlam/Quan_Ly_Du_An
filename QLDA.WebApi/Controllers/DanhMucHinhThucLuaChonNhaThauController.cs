using System.Net.Mime;
using QLDA.Application.DmHinhThucLuaChonNhaThaus.Queries;
using QLDA.Domain.Constants;
using QLDA.WebApi.Models.DanhMucHinhThucLuaChonNhaThaus;

namespace QLDA.WebApi.Controllers {
    [Tags("Danh mục hình thức lựa chọn nhà thầu")]
    [Route("api/danh-muc-hinh-thuc-lua-chon-nha-thau")]
    [Authorize(Roles = RoleConstants.GroupAdminOrManager)]
    public class DanhMucHinhThucLuaChonNhaThauController(IServiceProvider serviceProvider)
        : AggregateRootController(serviceProvider) {
        [ProducesResponseType<ResultApi<DanhMucHinhThucLuaChonNhaThauModel>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
        [HttpGet("{id}")]
        public async Task<ResultApi> Get(int id) {
            var entity = await Mediator.Send(new DanhMucGetQuery{Id = id.ToString(), DanhMuc = EDanhMuc.DanhMucHinhThucLuaChonNhaThau,ThrowIfNull = true, }) as DanhMucHinhThucLuaChonNhaThau;
            var model = entity!.ToModel();
            return ResultApi.Ok(model);
        }
        /// <summary>
        /// Tạm ẩn
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ProducesResponseType<ResultApi<DanhMucHinhThucLuaChonNhaThau>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
        [HttpDelete("xoa-tam")]
        public async Task<ResultApi> SoftDelete(int id) {
            var entity = await Mediator.Send(new DanhMucGetQuery{Id = id.ToString(), DanhMuc = EDanhMuc.DanhMucHinhThucLuaChonNhaThau,ThrowIfNull = true, }) as DanhMucHinhThucLuaChonNhaThau;
            entity!.IsDeleted = true;
            await Mediator.Send(new DanhMucInsertOrUpdateCommand(entity, EDanhMuc.DanhMucHinhThucLuaChonNhaThau));
            return ResultApi.Ok(entity);
        }

        [ProducesResponseType<ResultApi<PaginatedList<DanhMucDto<int>>>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
        [HttpGet("danh-sach-day-du")]
        public async Task<ResultApi> GetAll([FromQuery] AggregateRootPagination req, string? globalFilter) {
            var res = await Mediator.Send(new DanhMucGetDanhSachQuery() {
                DanhMuc = EDanhMuc.DanhMucHinhThucLuaChonNhaThau,
                PageIndex = req.PageIndex, GlobalFilter = globalFilter,
                PageSize = req.PageSize,
                GetAll = true
            });
            return ResultApi.Ok(res);
        }

        [ProducesResponseType<ResultApi<List<DanhMucHinhThucLuaChonNhaThau>>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
        [HttpGet("danh-sach")]
        public async Task<ResultApi> Get([FromQuery]bool getAll = false) {
            var res = await Mediator.Send(new DmHinhThucLuaChonNhaThauGetDanhSachQuery() {
           // var res = await Mediator.Send(new DanhMucGetDanhSachQuery() {
          //      DanhMuc = EDanhMuc.DanhMucHinhThucLuaChonNhaThau,
            //    Ids = ids,
                GetAll = getAll,
            }) ;
            return ResultApi.Ok(res == null ? [] :res.Data);
        }


        [ProducesResponseType<ResultApi<int>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
        [HttpPost("them-moi")]
        [Consumes(MediaTypeNames.Application.Json)]
        public async Task<ResultApi> Create([FromBody] DanhMucHinhThucLuaChonNhaThauModel model) {
            var entity = model.ToEntity();
            await Mediator.Send(new DanhMucInsertOrUpdateCommand(entity, EDanhMuc.DanhMucHinhThucLuaChonNhaThau));
            return ResultApi.Ok(1);
        }

        [ProducesResponseType<ResultApi<DanhMucHinhThucLuaChonNhaThauModel>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
        [HttpPut("cap-nhat")]
        [Consumes(MediaTypeNames.Application.Json)]
        public async Task<ResultApi> Update([FromBody] DanhMucHinhThucLuaChonNhaThauModel model) {
            var entity = await Mediator.Send(new DanhMucGetQuery{Id = model.GetId().ToString(), DanhMuc = EDanhMuc.DanhMucHinhThucLuaChonNhaThau,ThrowIfNull = true, }) as DanhMucHinhThucLuaChonNhaThau;

            entity!.Ma = model.Ma;
            entity.Ten = model.Ten;
            entity.MoTa = model.MoTa;
            entity.Stt = model.Stt;entity.Used = model.Used;

            await Mediator.Send(new DanhMucInsertOrUpdateCommand(entity, EDanhMuc.DanhMucHinhThucLuaChonNhaThau));

            return ResultApi.Ok(model);
        }
    }
}