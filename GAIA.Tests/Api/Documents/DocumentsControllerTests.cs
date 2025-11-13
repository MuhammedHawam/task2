using GAIA.Api.Contracts.Documents;
using GAIA.Api.Controllers;
using GAIA.Core.Documents.Interfaces;
using GAIA.Core.Documents.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace GAIA.Tests.Api.Documents;

public class DocumentsControllerTests
{
  [Fact]
  public async Task GetAll_ReturnsDocumentsWrappedResponse()
  {
    // Arrange
    var summaries = new List<DocumentSummaryDto>
    {
      new(Guid.NewGuid(), "Approved", "Policy", "Document A"),
      new(Guid.NewGuid(), "Draft", "Guideline", "Document B"),
    };

    var service = new StubDocumentService
    {
      GetAllHandler = _ => Task.FromResult(new DocumentCollectionDto(summaries))
    };

    var controller = new DocumentsController(service);

    // Act
    var result = await controller.GetAll(CancellationToken.None);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result.Result);
    var response = Assert.IsType<DocumentsResponse>(okResult.Value);
    Assert.Equal(2, response.Documents.Count);
    Assert.Equal("Approved", response.Documents[0].Status);
    Assert.Equal("Guideline", response.Documents[1].Category);
  }

  [Fact]
  public async Task GetById_ReturnsDocumentWithBase64Content()
  {
    // Arrange
    var documentId = Guid.NewGuid();
    var content = new byte[] { 1, 2, 3 };

    var service = new StubDocumentService
    {
      GetByIdHandler = (id, _) => Task.FromResult<DocumentDto?>(
        id == documentId
          ? new DocumentDto(documentId, content, "Approved", "Policy", "Document A")
          : null)
    };

    var controller = new DocumentsController(service);

    // Act
    var result = await controller.GetById(documentId, CancellationToken.None);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result.Result);
    var response = Assert.IsType<DocumentResponse>(okResult.Value);
    Assert.Equal(Convert.ToBase64String(content), response.Content);
    Assert.Equal("Document A", response.Name);
  }

  [Fact]
  public async Task GetById_WhenMissing_ReturnsNotFound()
  {
    // Arrange
    var service = new StubDocumentService();
    var controller = new DocumentsController(service);

    // Act
    var result = await controller.GetById(Guid.NewGuid(), CancellationToken.None);

    // Assert
    Assert.IsType<NotFoundResult>(result.Result);
  }

  [Fact]
  public async Task Create_PersistsAndReturnsCreatedDocument()
  {
    // Arrange
    var createdId = Guid.NewGuid();
    CreateDocumentDto? capturedCreate = null;

    var service = new StubDocumentService
    {
      CreateHandler = (dto, _) =>
      {
        capturedCreate = dto;
        return Task.FromResult(createdId);
      },
      GetByIdHandler = (id, _) => Task.FromResult<DocumentDto?>(
        id == createdId
          ? new DocumentDto(createdId, capturedCreate!.Content, capturedCreate.Status, capturedCreate.Category, capturedCreate.Name)
          : null)
    };

    var controller = new DocumentsController(service);

    var request = new CreateDocumentRequest(
      Convert.ToBase64String(new byte[] { 10, 20, 30 }),
      "Approved",
      "Policy",
      "Document A");

    // Act
    var result = await controller.Create(request, CancellationToken.None);

    // Assert
    var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
    var response = Assert.IsType<DocumentResponse>(createdResult.Value);

    Assert.Equal(createdId, response.Id);
    Assert.Equal(request.Status, response.Status);
    Assert.NotNull(capturedCreate);
    Assert.Equal(new byte[] { 10, 20, 30 }, capturedCreate!.Content);
  }

  [Fact]
  public async Task Create_WithInvalidBase64_ReturnsValidationProblem()
  {
    // Arrange
    var service = new StubDocumentService();
    var controller = new DocumentsController(service);

    var request = new CreateDocumentRequest(
      "invalid-base64",
      "Approved",
      "Policy",
      "Document A");

    // Act
    var result = await controller.Create(request, CancellationToken.None);

    // Assert
    var problemResult = Assert.IsType<ObjectResult>(result.Result);
    Assert.Equal(400, problemResult.StatusCode);
    Assert.IsType<ValidationProblemDetails>(problemResult.Value);
  }

  [Fact]
  public async Task Upload_SavesFileContentAndReturnsCreatedDocument()
  {
    // Arrange
    var createdId = Guid.NewGuid();
    CreateDocumentDto? capturedCreate = null;

    var service = new StubDocumentService
    {
      CreateHandler = (dto, _) =>
      {
        capturedCreate = dto;
        return Task.FromResult(createdId);
      },
      GetByIdHandler = (id, _) => Task.FromResult<DocumentDto?>(
        id == createdId
          ? new DocumentDto(createdId, capturedCreate!.Content, capturedCreate.Status, capturedCreate.Category, capturedCreate.Name)
          : null)
    };

    var controller = new DocumentsController(service);

    var fileBytes = new byte[] { 5, 6, 7, 8 };
    await using var stream = new MemoryStream(fileBytes);
    stream.Position = 0;
    var formFile = new FormFile(stream, 0, fileBytes.Length, "file", "test.bin");

    var request = new UploadDocumentRequest
    {
      File = formFile,
      Status = "Approved",
      Category = "Policy",
      Name = "Document A"
    };

    // Act
    var result = await controller.Upload(request, CancellationToken.None);

    // Assert
    var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
    var response = Assert.IsType<DocumentResponse>(createdResult.Value);

    Assert.Equal(createdId, response.Id);
    Assert.NotNull(capturedCreate);
    Assert.Equal(fileBytes, capturedCreate!.Content);
  }

  [Fact]
  public async Task Upload_WithMissingFile_ReturnsValidationProblem()
  {
    // Arrange
    var controller = new DocumentsController(new StubDocumentService());
    var request = new UploadDocumentRequest
    {
      File = null!,
      Status = "Approved",
      Category = "Policy",
      Name = "Document A"
    };

    // Act
    var result = await controller.Upload(request, CancellationToken.None);

    // Assert
    var problemResult = Assert.IsType<ObjectResult>(result.Result);
    Assert.Equal(400, problemResult.StatusCode);
  }

  [Fact]
  public async Task Update_WithContent_ReturnsNoContent()
  {
    // Arrange
    UpdateDocumentDto? capturedUpdate = null;

    var service = new StubDocumentService
    {
      UpdateHandler = (id, dto, _) =>
      {
        capturedUpdate = dto;
        return Task.FromResult(true);
      }
    };

    var controller = new DocumentsController(service);
    var request = new UpdateDocumentRequest(
      "Approved",
      "Policy",
      "Document A",
      Convert.ToBase64String(new byte[] { 1, 2, 3 }));

    // Act
    var result = await controller.Update(Guid.NewGuid(), request, CancellationToken.None);

    // Assert
    Assert.IsType<NoContentResult>(result);
    Assert.NotNull(capturedUpdate);
    Assert.Equal(new byte[] { 1, 2, 3 }, capturedUpdate!.Content);
  }

  [Fact]
  public async Task Update_WithInvalidBase64_ReturnsValidationProblem()
  {
    // Arrange
    var controller = new DocumentsController(new StubDocumentService());
    var request = new UpdateDocumentRequest(
      "Approved",
      "Policy",
      "Document A",
      "invalid-base64");

    // Act
    var result = await controller.Update(Guid.NewGuid(), request, CancellationToken.None);

    // Assert
    var problemResult = Assert.IsType<ObjectResult>(result);
    Assert.Equal(400, problemResult.StatusCode);
  }

  [Fact]
  public async Task Update_WhenDocumentMissing_ReturnsNotFound()
  {
    // Arrange
    var service = new StubDocumentService
    {
      UpdateHandler = (_, _, _) => Task.FromResult(false)
    };

    var controller = new DocumentsController(service);
    var request = new UpdateDocumentRequest("Approved", "Policy", "Document A", null);

    // Act
    var result = await controller.Update(Guid.NewGuid(), request, CancellationToken.None);

    // Assert
    Assert.IsType<NotFoundResult>(result);
  }

  [Fact]
  public async Task Delete_WhenSuccessful_ReturnsNoContent()
  {
    // Arrange
    var service = new StubDocumentService
    {
      DeleteHandler = (_, _) => Task.FromResult(true)
    };

    var controller = new DocumentsController(service);

    // Act
    var result = await controller.Delete(Guid.NewGuid(), CancellationToken.None);

    // Assert
    Assert.IsType<NoContentResult>(result);
  }

  [Fact]
  public async Task Delete_WhenMissing_ReturnsNotFound()
  {
    // Arrange
    var controller = new DocumentsController(new StubDocumentService());

    // Act
    var result = await controller.Delete(Guid.NewGuid(), CancellationToken.None);

    // Assert
    Assert.IsType<NotFoundResult>(result);
  }

  private sealed class StubDocumentService : IDocumentService
  {
    public Func<CancellationToken, Task<DocumentCollectionDto>> GetAllHandler { get; init; } =
      _ => Task.FromResult(new DocumentCollectionDto(Array.Empty<DocumentSummaryDto>()));

    public Func<Guid, CancellationToken, Task<DocumentDto?>> GetByIdHandler { get; init; } =
      (_, _) => Task.FromResult<DocumentDto?>(null);

    public Func<CreateDocumentDto, CancellationToken, Task<Guid>> CreateHandler { get; init; } =
      (_, _) => Task.FromResult(Guid.Empty);

    public Func<Guid, UpdateDocumentDto, CancellationToken, Task<bool>> UpdateHandler { get; init; } =
      (_, _, _) => Task.FromResult(false);

    public Func<Guid, CancellationToken, Task<bool>> DeleteHandler { get; init; } =
      (_, _) => Task.FromResult(false);

    public Task<DocumentCollectionDto> GetAllAsync(CancellationToken cancellationToken)
      => GetAllHandler(cancellationToken);

    public Task<DocumentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
      => GetByIdHandler(id, cancellationToken);

    public Task<Guid> CreateAsync(CreateDocumentDto document, CancellationToken cancellationToken)
      => CreateHandler(document, cancellationToken);

    public Task<bool> UpdateAsync(Guid id, UpdateDocumentDto document, CancellationToken cancellationToken)
      => UpdateHandler(id, document, cancellationToken);

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
      => DeleteHandler(id, cancellationToken);
  }
}
