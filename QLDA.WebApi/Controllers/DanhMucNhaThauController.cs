using System.Net.Mime;
using QLDA.Application.DanhMucNhaThaus.DTOs;
using QLDA.Application.DanhMucNhaThaus.Queries;
using QLDA.Application.NhaThauNguoiDungs.Commands;
using QLDA.Application.NhaThauNguoiDungs.DTOs;
using QLDA.Application.NhaThauNguoiDungs.Queries;
using QLDA.WebApi.Models.DanhMucNhaThaus;

namespace QLDA.WebApi.Controllers {
    [Tags("Danh mục nhà thầu")]
    [Route("api/danh-muc-nha-thau")]
    public class DanhMucNhaThauController : AggregateRootController {
        public DanhMucNhaThauController(IServiceProvider serviceProvider) : base(serviceProvider) {
        }

        [ProducesResponseType<ResultApi<DanhMucNhaThauModel>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
        [HttpGet("{id}")]
        public async Task<ResultApi> Get(Guid id) {
            var entity = await Mediator.Send(new DanhMucGetQuery { Id = id.ToString(), DanhMuc = EDanhMuc.DanhMucNhaThau, ThrowIfNull = true, }) as DanhMucNhaThau;
            var model = entity!.ToModel();
            // Lấy danh sách người dùng
            model.NguoiDungs = await Mediator.Send(new NhaThauNguoiDungGetListByNhaThauIdQuery { NhaThauId = id });
            model.NguoiDungIds = [.. model.NguoiDungs.Select(x => x.NguoiDungId)];
            return ResultApi.Ok(model);
        }


        /// <summary>
        /// Tạm ẩn
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ProducesResponseType<ResultApi<DanhMucNhaThau>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
        [HttpDelete("xoa-tam")]
        public async Task<ResultApi> SoftDelete(Guid id) {
            var entity = await Mediator.Send(new DanhMucGetQuery { Id = id.ToString(), DanhMuc = EDanhMuc.DanhMucNhaThau, ThrowIfNull = true, }) as DanhMucNhaThau;
            entity!.IsDeleted = true;
            await Mediator.Send(new DanhMucInsertOrUpdateCommand(entity, EDanhMuc.DanhMucNhaThau));
            return ResultApi.Ok(entity);
        }

        [ProducesResponseType<ResultApi<PaginatedList<DanhMucNhaThauDto>>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
        [HttpGet("danh-sach-day-du")]
        public async Task<ResultApi> GetAll([FromQuery] AggregateRootPagination req, string? globalFilter) {
            var res = await Mediator.Send(new DanhMucNhaThauGetDanhSachQuery() {
                PageIndex = req.PageIndex,
                GlobalFilter = globalFilter,
                PageSize = req.PageSize,
                GetAll = true,
            });
            return ResultApi.Ok(res);
        }

        [ProducesResponseType<ResultApi<List<DanhMucNhaThauDto>>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
        [HttpGet("danh-sach")]
        public async Task<ResultApi> Get([FromQuery] List<Guid>? ids = null, bool getAll = false) {
            var res = await Mediator.Send(new DanhMucNhaThauGetDanhSachQuery() {
                PageIndex = 0,
                PageSize = 0,
                Ids = ids,
                GetAll = getAll,
            });
            return ResultApi.Ok(res.Data);
        }


        [ProducesResponseType<ResultApi<int>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
        [HttpPost("them-moi")]
        [Consumes(MediaTypeNames.Application.Json)]
        public async Task<ResultApi> Create([FromBody] DanhMucNhaThauInsertModel model) {
            var entity = model.ToEntity();
            //Thêm nhà thầu
            await Mediator.Send(new DanhMucInsertOrUpdateCommand(entity, EDanhMuc.DanhMucNhaThau));
            //Thêm người dùng
            if (model.NguoiDungIds.Count > 0) {
                await Mediator.Send(new NhaThauNguoiDungSyncCommand {
                    NhaThauId = entity.Id,
                    NguoiDungIds = model.NguoiDungIds
                });
            }

            return ResultApi.Ok(entity.Id);
        }

        [ProducesResponseType<ResultApi<DanhMucNhaThauModel>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
        [HttpPut("cap-nhat")]
        [Consumes(MediaTypeNames.Application.Json)]
        public async Task<ResultApi> Update([FromBody] DanhMucNhaThauModel model) {
            var entity = await Mediator.Send(new DanhMucGetQuery { Id = model.GetId().ToString(), DanhMuc = EDanhMuc.DanhMucNhaThau, ThrowIfNull = true, }) as
                DanhMucNhaThau;

            entity!.Ma = model.Ma;
            entity.Ten = model.Ten;
            entity.MoTa = model.MoTa;
            entity.Stt = model.Stt;
            entity.Used = model.Used;
            entity.DiaChi = model.DiaChi;
            entity.MaSoThue = model.MaSoThue;
            entity.Email = model.Email;
            entity.SoDienThoai = model.SoDienThoai;
            entity.NguoiDaiDien = model.NguoiDaiDien;

            await Mediator.Send(new DanhMucInsertOrUpdateCommand(entity, EDanhMuc.DanhMucNhaThau));

            // Đồng bộ người dùng
            await Mediator.Send(new NhaThauNguoiDungSyncCommand {
                NhaThauId = model.GetId(),
                NguoiDungIds = model.NguoiDungIds
            });

            return ResultApi.Ok(model);
        }

        #region NhaThauNguoiDung

        /// <summary>
        /// Lấy danh sách người dùng thuộc nhà thầu
        /// </summary>
        [ProducesResponseType<ResultApi<List<NhaThauNguoiDungDto>>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
        [HttpGet("{nhaThauId}/nguoi-dung")]
        public async Task<ResultApi> GetNguoiDungs(Guid nhaThauId) {
            var result = await Mediator.Send(new NhaThauNguoiDungGetListByNhaThauIdQuery { NhaThauId = nhaThauId });
            return ResultApi.Ok(result);
        }

        /// <summary>
        /// Đồng bộ danh sách người dùng cho nhà thầu (thêm/xóa nhiều người dùng cùng lúc)
        /// </summary>
        [ProducesResponseType<ResultApi<int>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
        [HttpPost("{nhaThauId}/nguoi-dung/dong-bo")]
        [Consumes(MediaTypeNames.Application.Json)]
        public async Task<ResultApi> SyncNguoiDungs(Guid nhaThauId, [FromBody] List<long> nguoiDungIds) {
            var result = await Mediator.Send(new NhaThauNguoiDungSyncCommand {
                NhaThauId = nhaThauId,
                NguoiDungIds = nguoiDungIds
            });
            return ResultApi.Ok(result);
        }

        #endregion
    }
}
