using BuildingBlocks.Application.DmChucVus.DTOs;
using BuildingBlocks.Application.DmChucVus.Queries;
using BuildingBlocks.Application.UserMasters.Queries;
using QLDA.Application.DanhMucDonVis.Queries;
using QLDA.Application.DmChucVus.Queries;
using QLDA.Domain.Constants;
using QLDA.WebApi.Models.DmChucVus;
using System.Net.Mime;

namespace QLDA.WebApi.Controllers {
    [Tags("Danh mục chức vụ")]
    [Route("api/danh-muc-chuc-vu")]
    [Authorize(Roles = RoleConstants.GroupAdminOrManager)]
    public class DanhMucChucVuController(IServiceProvider serviceProvider) : AggregateRootController(serviceProvider) {
        [ProducesResponseType<ResultApi<DanhMucChucVuModel>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
        [HttpGet("{id}")]
        public async Task<ResultApi> Get(int id) {
            var entity = await Mediator.Send(new DanhMucGetQuery { Id = id.ToString(), DanhMuc = EDanhMuc.DanhMucChucVu, ThrowIfNull = true, }) as DanhMucChucVu;
            var model = entity!.ToModel();
            return ResultApi.Ok(model);
        }


        /// <summary>
        /// Tạm ẩn
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ProducesResponseType<ResultApi<DanhMucChucVu>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
        [HttpDelete("xoa-tam")]
        public async Task<ResultApi> SoftDelete(int id) {
            var entity = await Mediator.Send(new DanhMucGetQuery { Id = id.ToString(), DanhMuc = EDanhMuc.DanhMucChucVu, ThrowIfNull = true, }) as DanhMucChucVu;
            entity!.IsDeleted = true;
            await Mediator.Send(new DanhMucInsertOrUpdateCommand(entity, EDanhMuc.DanhMucChucVu));
            return ResultApi.Ok(entity);
        }

        [ProducesResponseType<ResultApi<PaginatedList<DanhMucDto<int>>>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
        [HttpGet("danh-sach-day-du")]
        public async Task<ResultApi> GetAll([FromQuery] AggregateRootPagination req, string? globalFilter) {
            var res = await Mediator.Send(new DanhMucGetDanhSachQuery() {
                DanhMuc = EDanhMuc.DanhMucChucVu,
                PageIndex = req.PageIndex,
                GlobalFilter = globalFilter,
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
                DanhMuc = EDanhMuc.DanhMucChucVu,
                PageIndex = 0,
                PageSize = 0,
                Ids = ids,
                GetAll = getAll,
            }) as PaginatedList<DanhMucDto<int>>;
            return ResultApi.Ok(res == null ? [] : res.Data);
        }
       
        [ProducesResponseType<ResultApi>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
        [HttpGet("danh-sach-master")]
        public async Task<ResultApi> GetMaster([FromQuery] bool getAll = false)
        {
            var res = await Mediator.Send(new DmChucVuGetDanhSachQuery()
            {
                GetAll = getAll,
            });
            return ResultApi.Ok(res.Data);
        }
        //DmChucVu
        [ProducesResponseType<ResultApi<int>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
        [HttpPost("them-moi")]
        [Consumes(MediaTypeNames.Application.Json)]
        public async Task<ResultApi> Create([FromBody] DanhMucChucVuModel model) {
            var entity = model.ToEntity();
            await Mediator.Send(new DanhMucInsertOrUpdateCommand(entity, EDanhMuc.DanhMucChucVu));
            return ResultApi.Ok(1);
        }

        [ProducesResponseType<ResultApi<DanhMucChucVuModel>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
        [HttpPut("cap-nhat")]
        [Consumes(MediaTypeNames.Application.Json)]
        public async Task<ResultApi> Update([FromBody] DanhMucChucVuModel model) {
            var entity = await Mediator.Send(new DanhMucGetQuery { Id = model.GetId().ToString(), DanhMuc = EDanhMuc.DanhMucChucVu, ThrowIfNull = true, }) as DanhMucChucVu;

            entity!.Ma = model.Ma;
            entity.Ten = model.Ten;
            entity.MoTa = model.MoTa;
            entity.Stt = model.Stt;
            entity.Used = model.Used;

            await Mediator.Send(new DanhMucInsertOrUpdateCommand(entity, EDanhMuc.DanhMucChucVu));

            return ResultApi.Ok(model);
        }
    }
}