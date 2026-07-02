using FluentValidation;
using QLDA.Application.HoSoDeXuatCapDoCntts.Commands;
using QLDA.Application.HoSoDeXuatCapDoCntts.DTOs;

namespace QLDA.Application.HoSoDeXuatCapDoCntts.Validators;

public class HoSoDeXuatCapDoCnttInsertValidator : AbstractValidator<HoSoDeXuatCapDoCnttInsertCommand> {
    public HoSoDeXuatCapDoCnttInsertValidator() {
        RuleFor(x => x.Dto.DuAnId).NotEmpty().WithMessage("Dự án không được để trống");
        RuleFor(x => x.Dto.CapDoId).NotEmpty().WithMessage("Cấp độ không được để trống");
        RuleFor(x => x.Dto.NoiDungDeNghi).MaximumLength(2000);
        RuleFor(x => x.Dto.NoiDungBaoCao).MaximumLength(2000);
        RuleFor(x => x.Dto.NoiDungDuThao).MaximumLength(2000);
    }
}

public class HoSoDeXuatCapDoCnttUpdateValidator : AbstractValidator<HoSoDeXuatCapDoCnttUpdateCommand> {
    public HoSoDeXuatCapDoCnttUpdateValidator() {
        RuleFor(x => x.Model.Id).NotEmpty();
        RuleFor(x => x.Model.CapDoId).NotEmpty().WithMessage("Cấp độ không được để trống");
        RuleFor(x => x.Model.NoiDungDeNghi).MaximumLength(2000);
        RuleFor(x => x.Model.NoiDungBaoCao).MaximumLength(2000);
        RuleFor(x => x.Model.NoiDungDuThao).MaximumLength(2000);
    }
}

public class HoSoDeXuatCapDoCnttThayDoiTrangThaiValidator 
    : AbstractValidator<HoSoDeXuatCapDoCnttPheDuyetCommand> {
    
    public HoSoDeXuatCapDoCnttThayDoiTrangThaiValidator() {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id hồ sơ không được để trống");
        RuleFor(x => x.TrangThaiTiepTheo).NotEmpty().WithMessage("Trạng thái không được để trống");
    }

    private static bool IsTuChoi(int trangThaiId) {
        // TODO: Map trạng thái "Từ chối" từ database
        return false; // Placeholder
    }
}