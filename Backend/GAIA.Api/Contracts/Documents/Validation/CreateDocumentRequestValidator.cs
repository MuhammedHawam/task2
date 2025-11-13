using FluentValidation;

namespace GAIA.Api.Contracts.Documents.Validation;

public class CreateDocumentRequestValidator : AbstractValidator<CreateDocumentRequest>
{
  public CreateDocumentRequestValidator()
  {
    RuleFor(request => request.Content)
      .NotEmpty().WithMessage("Content is required.");

    RuleFor(request => request.Status)
      .NotEmpty()
      .MaximumLength(100);

    RuleFor(request => request.Category)
      .NotEmpty()
      .MaximumLength(100);

    RuleFor(request => request.Name)
      .NotEmpty()
      .MaximumLength(255);
  }
}
