using FluentValidation;

namespace GAIA.Api.Contracts.Validation
{
  public class CreateAssessmentRequestValidator : AbstractValidator<CreateAssessmentRequest>
  {
    public CreateAssessmentRequestValidator()
    {
      RuleFor(x => x.Title)
        .NotEmpty().WithMessage("Title is required.")
        .MaximumLength(200).WithMessage("Title must be at most 200 characters.");

      RuleFor(x => x.Description)
        .NotEmpty().WithMessage("Description is required.")
        .MaximumLength(5000).WithMessage("Description must be at most 5000 characters.");

      RuleFor(x => x.CreatedBy)
        .NotEmpty().WithMessage("CreatedBy is required.");

      RuleFor(x => x.FrameworkId)
        .NotEmpty().WithMessage("FrameworkId is required.");

      RuleFor(x => x.AssessmentDepthId)
     .NotEmpty().WithMessage("AssessmentDepthId is required.");

      RuleFor(x => x.AssessmentScoringId)
        .NotEmpty().WithMessage("AssessmentScoringId is required.");
    }
  }

}
