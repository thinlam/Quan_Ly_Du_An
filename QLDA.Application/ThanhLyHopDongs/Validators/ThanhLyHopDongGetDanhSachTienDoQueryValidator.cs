using QLDA.Application.ThanhLyHopDongs.Queries;

namespace QLDA.Application.ThanhLyHopDongs.Validators;

public class ThanhLyHopDongGetDanhSachTienDoQueryValidator : AbstractValidator<ThanhLyHopDongGetDanhSachTienDoQuery> {
    public ThanhLyHopDongGetDanhSachTienDoQueryValidator() {
        RuleFor(x => x.DuAnId)
            .NotEmpty().WithMessage("Dự án là bắt buộc");
        RuleFor(x => x.BuocId)
            .NotEmpty().WithMessage("Bước là bắt buộc");
    }
}
