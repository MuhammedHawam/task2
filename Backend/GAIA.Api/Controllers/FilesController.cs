using GAIA.Core.FileStorage.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GAIA.Api.Controllers;

[ApiController]
[Route("files")]
public class FilesController : ControllerBase
{
  private readonly ISender _sender;

  public FilesController(ISender sender)
  {
    _sender = sender;
  }

  [HttpGet("{fileId:guid}")]
  public async Task<IActionResult> Download(Guid fileId, CancellationToken cancellationToken)
  {
    var file = await _sender.Send(new DownloadStoredFileQuery(fileId), cancellationToken);
    if (file is null)
    {
      return NotFound();
    }

    return File(file.Content, file.ContentType, file.FileName);
  }
}
