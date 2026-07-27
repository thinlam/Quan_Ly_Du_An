namespace BuildingBlocks.Application.Attachments.Validators;

public class AttachmentBulkInsertOrUpdateCommandValidator
    : AbstractValidator<Commands.AttachmentBulkInsertOrUpdateCommand>
{
    public AttachmentBulkInsertOrUpdateCommandValidator()
    {
        RuleFor(x => x.GroupId)
            .NotEmpty()
            .WithMessage("GroupId là bắt buộc.");

        RuleFor(x => x.GroupTypes)
            .NotEmpty()
            .WithMessage("GroupTypes là bắt buộc để tránh xóa nhầm files khác cùng GroupId.");

        RuleFor(x => x.GroupTypes)
            .Must(list => list != null && list.Any(t => !string.IsNullOrWhiteSpace(t)))
            .WithMessage("GroupTypes phải chứa ít nhất 1 giá trị hợp lệ.");

        RuleFor(x => x.Entities)
            .NotNull()
            .WithMessage("Entities không được null.");

        RuleFor(x => x)
            .Must(cmd => cmd.Entities == null
                || cmd.Entities.All(e =>
                    string.IsNullOrEmpty(e.GroupId) || e.GroupId == cmd.GroupId))
            .WithMessage("Tất cả file phải cùng GroupId với command.");
    }
}
