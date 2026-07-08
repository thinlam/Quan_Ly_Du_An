using FluentValidation;
using QLDA.Application.ThanhLyHopDongs.Commands;

namespace QLDA.Application.ThanhLyHopDongs.Validators;

public class ThanhLyHopDongUpdateCommandValidator : AbstractValidator<ThanhLyHopDongUpdateCommand> {
    public ThanhLyHopDongUpdateCommandValidator() {
        RuleFor(x => x.Dto.Id)
            .NotEmpty().WithMessage("Id là bắt buộc");
        RuleFor(x => x.Dto.DuAnId)
            .NotEmpty().WithMessage("Dự án là bắt buộc");
    }
}
