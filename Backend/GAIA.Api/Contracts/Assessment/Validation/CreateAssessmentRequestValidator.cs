using FluentValidation;

namespace GAIA.Api.Contracts.Assessment.Validation;

public class CreateAssessmentRequestValidator : AbstractValidator<CreateAssessmentRequest>
{
  public CreateAssessmentRequestValidator()
  {
    RuleFor(x => x.Name)
      .NotEmpty().WithMessage("Name is required.")
      .MaximumLength(200).WithMessage("Name must be at most 200 characters.");

    RuleFor(x => x.OrganizationId)
      .NotEmpty().WithMessage("OrganizationId is required.");

    RuleFor(x => x.Organization)
      .NotEmpty().WithMessage("Organization is required.")
      .MaximumLength(200).WithMessage("Organization must be at most 200 characters.");

    RuleFor(x => x.Language)
      .NotEmpty().WithMessage("Language is required.")
      .MaximumLength(100).WithMessage("Language must be at most 100 characters.");

    RuleFor(x => x.StartDate)
      .NotEmpty().WithMessage("StartDate is required.");

    RuleFor(x => x.EndDate)
      .NotEmpty().WithMessage("EndDate is required.")
      .GreaterThanOrEqualTo(x => x.StartDate)
      .WithMessage("EndDate must be on or after StartDate.");
  }
}
