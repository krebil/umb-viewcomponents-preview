using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace BlazorExample.Site.Controllers.Api;

public class UtilitiesApiController  : ControllerBase
{
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;

    public UtilitiesApiController(IUmbracoContextAccessor umbracoContextAccessor)
    {
        _umbracoContextAccessor = umbracoContextAccessor;
    }

    [HttpGet]
    [Route("umbraco/_framework/{*path}")]
    public IActionResult GetBlazorFiles([FromRoute]string path)
    {
        return Redirect($"{Request.Scheme}://{Request.Host}/_framework/{path}");
    }

    [HttpGet]
    [Route("umbraco/_content/{*path}")]
    public IActionResult GetBlazorContentFiles([FromRoute]string path)
    {
        return Redirect($"{Request.Scheme}://{Request.Host}/_content/{path}");
    }
}