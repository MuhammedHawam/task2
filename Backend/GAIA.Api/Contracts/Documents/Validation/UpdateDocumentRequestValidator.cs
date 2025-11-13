using FluentValidation;

namespace GAIA.Api.Contracts.Documents.Validation;

public class UpdateDocumentRequestValidator : AbstractValidator<UpdateDocumentRequest>
{
  public UpdateDocumentRequestValidator()
  {
    RuleFor(request => request.Status)
      .NotEmpty()
      .MaximumLength(100);

    RuleFor(request => request.Category)
      .NotEmpty()
      .MaximumLength(100);

    RuleFor(request => request.Name)
      .NotEmpty()
      .MaximumLength(255);

    RuleFor(request => request.Content)
      .NotEmpty()
      .When(request => request.Content is not null);
  }
}
