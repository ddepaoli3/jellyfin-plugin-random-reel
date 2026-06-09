using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Jellyfin.Plugin.RandomReel.Api;

/// <summary>
/// Serves the standalone TV web app.
/// </summary>
[ApiController]
[Route("RandomReel")]
public class InjectController : ControllerBase
{
    /// <summary>
    /// Returns the standalone TV-friendly web app.
    /// Anonymous so it works without Jellyfin session.
    /// </summary>
    /// <returns>HTML page.</returns>
    [HttpGet("app")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetTvApp()
    {
        var assembly = Assembly.GetExecutingAssembly();
        const string ResourceName = "Jellyfin.Plugin.RandomReel.Web.tvapp.html";

        var stream = assembly.GetManifestResourceStream(ResourceName);
        if (stream is null)
        {
            return NotFound("tvapp.html resource not found in assembly.");
        }

        using var reader = new StreamReader(stream);
        var content = reader.ReadToEnd();
        return Content(content, "text/html");
    }
}
