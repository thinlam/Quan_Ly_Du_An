using QLDA.Application.BaoCaoTienDos.Commands;
using QLDA.Application.GoiThaus.Commands;
using QLDA.WebApi.Models.BaoCaoTienDos;
using QLDA.WebApi.Models.GoiThaus;

namespace QLDA.WebApi.Controllers;

[Tags("Import")]
[Route("api/import")]
public class ImportController(IServiceProvider serviceProvider) : AggregateRootController(serviceProvider) {
    private readonly IImporterHelper _excelImporter = serviceProvider.GetRequiredService<IImporterHelper>();
    [HttpPost("bao-cao-tien-do")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType<ResultApi>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> ImportBaoCaoTienDo() {
        var formFile = await Request.ReadFormAsync();

        var file = formFile.Files.FirstOrDefault();

        if (file == null || file.Length == 0)
            return ResultApi.Fail("File không hợp lệ");

        var data = _excelImporter.ReadDataFromExcel<BaoCaoTienDoImportModel>(file.OpenReadStream());

        var query = new BaoCaoTienDoImportRangeCommand(data.ToImportDtoList());

        await Mediator.Send(query);

        return ResultApi.Ok(data);
    }

    [HttpPost("goi-thau")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ResultApi), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultApi), StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> ImportGoiThau() {
        var formFile = await Request.ReadFormAsync();

        var file = formFile.Files.FirstOrDefault();

        if (file == null || file.Length == 0)
            return ResultApi.Fail("File không hợp lệ");

        var data = _excelImporter.ReadDataFromExcel<GoiThauImportModel>(file.OpenReadStream());

        var query = new GoiThauImportRangeCommand(data.ToImportDtoList());

        await Mediator.Send(query);

        return ResultApi.Ok(data);
    }
}