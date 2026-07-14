using QLDA.Application.ThanhLyHopDongs.Commands;

namespace QLDA.Application.ThanhLyHopDongs.Validators;

public class ThanhLyHopDongInsertCommandValidator : AbstractValidator<ThanhLyHopDongInsertCommand> {
    public ThanhLyHopDongInsertCommandValidator() {
        RuleFor(x => x.Dto.DuAnId)
            .NotEmpty().WithMessage("Dự án là bắt buộc");
    }
}
