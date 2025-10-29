using FluentValidation;

namespace GAIA.Domain.Assessment.Validation
{
  public class AssessmentValidator : AbstractValidator<Entities.Assessment>
  {
    public AssessmentValidator()
    {
      RuleFor(x => x.Title)
        .NotEmpty()
        .WithMessage("Title is required.");

      RuleFor(x => x.FrameworkId)
        .Must(id => id != Guid.Empty)
        .WithMessage("FrameworkId is required.");

      RuleFor(x => x.CreatedBy)
        .Must(id => id != Guid.Empty)
        .WithMessage("CreatedBy must be provided.");
    }
  }

}
