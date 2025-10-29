using System;
using GAIA.Domain.Assessment.Entities;
using GAIA.Domain.Assessment.Validation;
using Xunit;

namespace GAIA.Tests.Domain.Assessment;

public class AssessmentValidatorTests
{
  [Fact]
  public void Validate_ReturnsValid_ForCorrectEntity()
  {
    var entity = new Assessment
    {
      Id = Guid.NewGuid(),
      Title = "Security Review",
      Description = "Annual security posture review.",
      CreatedAt = DateTime.UtcNow,
      CreatedBy = Guid.NewGuid(),
      FrameworkId = Guid.NewGuid()
    };

    var validator = new AssessmentValidator();

    var result = validator.Validate(entity);

    Assert.True(result.IsValid);
  }

  [Fact]
  public void Validate_Fails_WhenTitleMissing()
  {
    var entity = new Assessment
    {
      Id = Guid.NewGuid(),
      Title = string.Empty,
      Description = "Desc",
      CreatedAt = DateTime.UtcNow,
      CreatedBy = Guid.NewGuid(),
      FrameworkId = Guid.NewGuid()
    };

    var validator = new AssessmentValidator();

    var result = validator.Validate(entity);

    Assert.False(result.IsValid);
    Assert.Contains(result.Errors, e => e.PropertyName == nameof(Assessment.Title));
  }

  [Fact]
  public void Validate_Fails_WhenFrameworkIdEmpty()
  {
    var entity = new Assessment
    {
      Id = Guid.NewGuid(),
      Title = "Title",
      Description = "Desc",
      CreatedAt = DateTime.UtcNow,
      CreatedBy = Guid.NewGuid(),
      FrameworkId = Guid.Empty
    };

    var validator = new AssessmentValidator();

    var result = validator.Validate(entity);

    Assert.False(result.IsValid);
    Assert.Contains(result.Errors, e => e.PropertyName == nameof(Assessment.FrameworkId));
  }

  [Fact]
  public void Validate_Fails_WhenCreatedByEmpty()
  {
    var entity = new Assessment
    {
      Id = Guid.NewGuid(),
      Title = "Title",
      Description = "Desc",
      CreatedAt = DateTime.UtcNow,
      CreatedBy = Guid.Empty,
      FrameworkId = Guid.NewGuid()
    };

    var validator = new AssessmentValidator();

    var result = validator.Validate(entity);

    Assert.False(result.IsValid);
    Assert.Contains(result.Errors, e => e.PropertyName == nameof(Assessment.CreatedBy));
  }
}
