using FluentValidation;
using QLDA.Application.BaoCaoKetQuaKhaoSats.Commands;

namespace QLDA.Application.BaoCaoKetQuaKhaoSats.Validators;

public class BaoCaoKetQuaKhaoSatInsertValidator : AbstractValidator<BaoCaoKetQuaKhaoSatInsertCommand>
{
    public BaoCaoKetQuaKhaoSatInsertValidator()
    {
        RuleFor(x => x.Dto.DuAnId).NotEmpty().WithMessage("Dự án không được để trống");
        RuleFor(x => x.Dto.NoiDungBaoCao).MaximumLength(4000);
        RuleFor(x => x.Dto.NoiDungNghiemThu).MaximumLength(4000);
    }
}

public class BaoCaoKetQuaKhaoSatUpdateValidator : AbstractValidator<BaoCaoKetQuaKhaoSatUpdateCommand>
{
    public BaoCaoKetQuaKhaoSatUpdateValidator()
    {
        RuleFor(x => x.Model.Id).NotEmpty();
        RuleFor(x => x.Model.NoiDungBaoCao).MaximumLength(4000);
        RuleFor(x => x.Model.NoiDungNghiemThu).MaximumLength(4000);
    }
}
