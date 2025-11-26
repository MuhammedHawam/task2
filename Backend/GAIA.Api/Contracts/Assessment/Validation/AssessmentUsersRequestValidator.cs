using FluentValidation;
using GAIA.Api.Contracts.Assessment;

namespace GAIA.Api.Contracts.Assessment.Validation;

public class AssessmentUsersRequestValidator : AbstractValidator<AssessmentUsersRequest>
{
  public AssessmentUsersRequestValidator()
  {
    RuleFor(request => request.Users)
      .NotNull()
      .WithMessage("Users collection cannot be null.");

    RuleForEach(request => request.Users)
      .SetValidator(new AssessmentUserRequestValidator());
  }
}

public class AssessmentUserRequestValidator : AbstractValidator<AssessmentUserRequest>
{
  public AssessmentUserRequestValidator()
  {
    RuleFor(user => user.Id)
      .NotEmpty();

    RuleFor(user => user.Username)
      .NotEmpty();

    RuleFor(user => user.Email)
      .NotEmpty()
      .EmailAddress();

    RuleFor(user => user.Role)
      .NotEmpty();
  }
}
