using FluentValidation;
using GAIA.Api.Contracts;

namespace GAIA.Api.Contracts.Validation;

public class CreateAssessmentRequestValidator : AbstractValidator<CreateAssessmentRequest>
{
  public CreateAssessmentRequestValidator()
  {
    RuleFor(x => x.Title)
      .Transform(title => title?.Trim() ?? string.Empty)
      .NotEmpty().WithMessage("Title is required.")
      .MaximumLength(200).WithMessage("Title must be at most 200 characters.");

    RuleFor(x => x.Description)
      .Transform(desc => desc?.Trim() ?? string.Empty)
      .NotEmpty().WithMessage("Description is required.")
      .MaximumLength(5000).WithMessage("Description must be at most 5000 characters.");

    RuleFor(x => x.CreatedBy)
      .NotEmpty().WithMessage("CreatedBy must be a non-empty GUID.");

    RuleFor(x => x.FrameworkId)
      .NotEmpty().WithMessage("FrameworkId must be a non-empty GUID.");
  }
}
