using FluentValidation;

namespace GAIA.Api.Contracts.Assessment.Validation;

public class UpdateInsightContentRequestValidator : AbstractValidator<UpdateInsightContentRequest>
{
  public const int MaxContentLength = 20_000;

  public UpdateInsightContentRequestValidator()
  {
    RuleFor(x => x.Content)
      .NotEmpty()
      .MaximumLength(MaxContentLength);
  }
}
