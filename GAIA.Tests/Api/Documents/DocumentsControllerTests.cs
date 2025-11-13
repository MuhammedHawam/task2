using GAIA.Api.Contracts.Documents;
using GAIA.Api.Controllers;
using GAIA.Core.Documents.Services;
using GAIA.Infra;
using GAIA.Infra.Repositories;
using GAIA.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace GAIA.Tests.Api.Documents
{
  public class DocumentsControllerTests
  {
    [Fact]
    public async Task GetDocuments_ReturnsSummaries()
    {
      using var context = CreateContext();
      var documentId = Guid.NewGuid();
      context.Documents.Add(new Document
      {
        Id = documentId,
        Name = "Strategy Plan",
        Category = "Planning",
        Status = "Draft",
        Content = new byte[] { 1, 2, 3 }
      });
      await context.SaveChangesAsync();

      var controller = CreateController(context);

      var result = await controller.GetDocuments(CancellationToken.None);

      var okResult = Assert.IsType<OkObjectResult>(result.Result);
      var response = Assert.IsType<DocumentListResponse>(okResult.Value);
      var summary = Assert.Single(response.Documents);
      Assert.Equal(documentId, summary.Id);
      Assert.Equal("Draft", summary.Status);
      Assert.Equal("Planning", summary.Category);
      Assert.Equal("Strategy Plan", summary.Name);
    }

    [Fact]
    public async Task GetById_ReturnsDocument()
    {
      using var context = CreateContext();
      var documentId = Guid.NewGuid();
      var content = new byte[] { 10, 20, 30 };
      context.Documents.Add(new Document
      {
        Id = documentId,
        Name = "Security Policy",
        Category = "Compliance",
        Status = "Approved",
        Content = content
      });
      await context.SaveChangesAsync();

      var controller = CreateController(context);

      var result = await controller.GetById(documentId, CancellationToken.None);

      var okResult = Assert.IsType<OkObjectResult>(result.Result);
      var response = Assert.IsType<DocumentResponse>(okResult.Value);
      Assert.Equal(documentId, response.Id);
      Assert.Equal("Approved", response.Status);
      Assert.Equal("Compliance", response.Category);
      Assert.Equal("Security Policy", response.Name);
      Assert.Equal(Convert.ToBase64String(content), response.ContentBase64);
    }

    [Fact]
    public async Task Create_PersistsDocument()
    {
      using var context = CreateContext();
      var controller = CreateController(context);
      var request = new CreateDocumentRequest(
        Name: "Architecture Diagram",
        Category: "Engineering",
        Status: "Draft",
        ContentBase64: Convert.ToBase64String(new byte[] { 11, 22, 33 }));

      var result = await controller.Create(request, CancellationToken.None);

      var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
      var response = Assert.IsType<DocumentResponse>(createdResult.Value);

      var saved = await context.Documents.SingleAsync(d => d.Id == response.Id);
      Assert.Equal("Architecture Diagram", saved.Name);
      Assert.Equal("Engineering", saved.Category);
      Assert.Equal("Draft", saved.Status);
      Assert.Equal(new byte[] { 11, 22, 33 }, saved.Content);

      Assert.Equal(saved.Id, response.Id);
      Assert.Equal(Convert.ToBase64String(saved.Content), response.ContentBase64);
    }

    [Fact]
    public async Task Upload_UsesFileContent()
    {
      using var context = CreateContext();
      var controller = CreateController(context);
      await using var contentStream = new MemoryStream(new byte[] { 5, 6, 7, 8 });
      var formFile = new FormFile(contentStream, 0, contentStream.Length, "file", "document.pdf")
      {
        Headers = new HeaderDictionary(),
        ContentType = "application/pdf"
      };
      contentStream.Position = 0;

      var request = new UploadDocumentRequest
      {
        File = formFile,
        Name = "Uploaded Document",
        Category = "Files",
        Status = "Uploaded"
      };

      var result = await controller.Upload(request, CancellationToken.None);

      var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
      var response = Assert.IsType<DocumentResponse>(createdResult.Value);

      var saved = await context.Documents.SingleAsync(d => d.Id == response.Id);
      Assert.Equal("Uploaded Document", saved.Name);
      Assert.Equal("Files", saved.Category);
      Assert.Equal("Uploaded", saved.Status);
      Assert.Equal(new byte[] { 5, 6, 7, 8 }, saved.Content);
    }

    [Fact]
    public async Task Update_ReplacesMetadataAndOptionallyContent()
    {
      using var context = CreateContext();
      var documentId = Guid.NewGuid();
      context.Documents.Add(new Document
      {
        Id = documentId,
        Name = "Original",
        Category = "Old",
        Status = "Initial",
        Content = new byte[] { 1, 1, 1 }
      });
      await context.SaveChangesAsync();

      var controller = CreateController(context);
      var request = new UpdateDocumentRequest(
        Name: "Updated",
        Category: "New",
        Status: "Reviewed",
        ContentBase64: Convert.ToBase64String(new byte[] { 9, 9, 9 }));

      var result = await controller.Update(documentId, request, CancellationToken.None);

      var okResult = Assert.IsType<OkObjectResult>(result.Result);
      var response = Assert.IsType<DocumentResponse>(okResult.Value);

      Assert.Equal(documentId, response.Id);
      Assert.Equal("Reviewed", response.Status);
      Assert.Equal("New", response.Category);
      Assert.Equal("Updated", response.Name);

      var saved = await context.Documents.SingleAsync(d => d.Id == documentId);
      Assert.Equal(new byte[] { 9, 9, 9 }, saved.Content);
    }

    [Fact]
    public async Task Delete_RemovesDocument()
    {
      using var context = CreateContext();
      var documentId = Guid.NewGuid();
      context.Documents.Add(new Document
      {
        Id = documentId,
        Name = "To Delete",
        Category = "Archive",
        Status = "Old",
        Content = new byte[] { 3, 3, 3 }
      });
      await context.SaveChangesAsync();

      var controller = CreateController(context);

      var result = await controller.Delete(documentId, CancellationToken.None);

      Assert.IsType<NoContentResult>(result);
      Assert.False(await context.Documents.AnyAsync(d => d.Id == documentId));
    }

    private static DocumentsController CreateController(ApplicationDbContext context)
    {
      var repository = new DocumentRepository(context);
      var service = new DocumentService(repository);
      return new DocumentsController(service);
    }

    private static ApplicationDbContext CreateContext()
    {
      var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;

      return new ApplicationDbContext(options);
    }
  }
}
