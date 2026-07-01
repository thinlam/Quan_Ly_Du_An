using QLDA.Application.PhanQuyenChucNangs.Commands;
using QLDA.Application.PhanQuyenChucNangs.DTOs;
using QLDA.Application.PhanQuyenChucNangs.Queries;
using QLDA.Domain.Constants;
using QLDA.WebApi.Models.PhanQuyenChucNangs;
using System.Net.Mime;

namespace QLDA.WebApi.Controllers;

[Tags("Phân quyền chức năng")]
[Route("api/phan-quyen-chuc-nang")]
public class PhanQuyenChucNangController : AggregateRootController
{
    // GET
    public PhanQuyenChucNangController(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    /// <summary>
    /// Chi tiết
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [ProducesResponseType<ResultApi<PhanQuyenChucNangDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("{id}/chi-tiet")]
    public async Task<ResultApi> Get(int id)
    {
        var entity = await Mediator.Send(new PhanQuyenChucNangGetById()
        {
            Id = id,
            ThrowIfNull = true,
            IsNoTracking = true,
        });


        return ResultApi.Ok(entity);
        // return ResultApi.Ok(entity.ToModel());
    }


    [HttpDelete("{id}/xoa")]
    public async Task<ResultApi> Delete(int id)
    {
        var res = await Mediator.Send(new PhanQuyenChucNangDeleteCommand(id));
        return ResultApi.Ok(res);
    }

    [HttpPost("them-moi")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<ResultApi<int>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> Create([FromBody] PhanQuyenChucNangModel model)
    {
        var entity = new PhanQuyenChucNang()
        {
            Level = (PhanQuyenChucNangLevel?)model.Level,// phương thức phòng ban/vai trò/ng dùng
            ChucNang = model.ChucNang,
            MaChucNang = model.MaChucNang,
            SuDung = model.SuDung,
            DanhSachChiTiet = model.DanhSachChiTiet?.Select(x=> x.ToEntity(model.Id??0)).ToList(),
          
        };
        var id = await Mediator.Send(new PhanQuyenChucNangInsertUpdateCommand(entity));
        return ResultApi.Ok(id);
    }


    [HttpPut("cap-nhat")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<ResultApi<PhanQuyenChucNang>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> Update(
        [FromBody] PhanQuyenChucNangModel model,
        [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken = default)
    {
        var entity = await Mediator.Send(new PhanQuyenChucNangGetQuery { Id = model.Id??0, ThrowIfNull = true });
        entity.Update(model);
        var result = await Mediator.Send(new PhanQuyenChucNangInsertUpdateCommand(entity), cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ResultApi.Ok(entity);
    }



    [HttpGet("danh-sach-tien-do")]
    [ProducesResponseType<ResultApi<PaginatedList<PhanQuyenChucNangDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> Get([FromQuery] PhanQuyenChucNangSearchDto req)
    {
        var res = await Mediator.Send(new PhanQuyenChucNangDanhSachQuery()
        {

            GlobalFilter = req.GlobalFilter,
            PageIndex = req.PageIndex,
            PageSize = req.PageSize,
            IsNoTracking = true,
            MaChucNang = req.MaChucNang,
            ChucNang = req.ChucNang,
            Level = req.Level,
            //LevelId = req.LevelId,

        });
        return ResultApi.Ok(res);
    }
}