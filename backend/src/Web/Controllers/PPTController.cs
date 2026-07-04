using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PPTRevive.Application.PPT.Command;
using PPTRevive.Application.PPT.Query;

namespace PPTRevive.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
// Free lead-magnet tier: the whole revive flow is usable with no account.
// Jobs are protected by their unguessable jobId (GUID); if a user IS signed in,
// per-user ownership is additionally enforced in the handlers.
[AllowAnonymous]
[EnableRateLimiting("ppt-anon")]
public class PPTController : ControllerBase
{
    private readonly ISender _sender;

    public PPTController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [Route("upload")]
    public async Task<IActionResult> UploadPPT([FromForm] PPTUploadCommand request)
    {
        var result = await _sender.Send(request);
        return result.Status ? Ok(result) : BadRequest(result);
    }

    [HttpGet]
    [Route("start")]
    public async Task<IActionResult> StartProcessing(string JobId)
    {
        var result = await _sender.Send(new PPTStartProcessingCommand()
        {
            JobId = JobId,
        });
        return result.Status ? Ok(result) : BadRequest(result);
    }

    [HttpGet]
    [Route("status")]
    public async Task<IActionResult> GetPPTStatus(string JobId)
    {
        var result = await _sender.Send(new GetPptProcessingStatusQuery()
        {
            JobId = JobId
        });
        return result.Status ? Ok(result) : BadRequest(result);
    }

    [HttpPost]
    [Route("finalize-slides")]
    public async Task<IActionResult> GetPPTStatus(PPTApproveCommand request)
    {
        var result = await _sender.Send(request);
        return Ok(result);
    }

    [HttpGet]
    [Route("download")]
    public async Task<IActionResult> Download(string jobId)
    {
        var result = await _sender.Send(new DownloadPptQuery { JobId = jobId });
        if (!result.Status || result.Data is not DownloadPptResult file)
            return NotFound(result);

        return File(
            file.Content,
            "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            file.FileName);
    }

    //[AllowAnonymous]
    //[HttpPost]
    //[Route("process")]
    //public async Task<IActionResult> ProcessPPT([FromForm] PptProcessCommand request)
    //{
    //    var result = await _sender.Send(request);
    //    return result.Status ? Ok(result) : BadRequest(result);
    //}
    
    [HttpPost]
    [Route("regenerate")]
    public async Task<IActionResult> RegeneratePPT([FromBody] PptRegenerateCommand request)
    {
        var result = await _sender.Send(request);
        return result.Status ? Ok(result) : BadRequest(result);
    }
}
