using FluentValidation;

namespace GAIA.Api.Contracts.Documents.Validation;

public class UploadDocumentRequestValidator : AbstractValidator<UploadDocumentRequest>
{
  public UploadDocumentRequestValidator()
  {
    RuleFor(request => request.File)
      .NotNull().WithMessage("File is required.")
      .DependentRules(() =>
      {
        RuleFor(request => request.File.Length)
          .GreaterThan(0)
          .WithMessage("File cannot be empty.");
      });

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
