using QLDA.Application.KetQuaTrungThaus.Commands;

namespace QLDA.Application.KetQuaTrungThaus.Validators;

public class KetQuaTrungThauInsertCommandValidator : AbstractValidator<KetQuaTrungThauInsertCommand> {
    public KetQuaTrungThauInsertCommandValidator() {
        // Issue #9643
        RuleFor(x => x.Dto.HinhThucHopDong)
            .MaximumLength(500).WithMessage("Hình thức hợp đồng không được vượt quá 500 ký tự");
    }
}