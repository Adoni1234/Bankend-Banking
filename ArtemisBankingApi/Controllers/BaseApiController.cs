using ArtemisBanking.Core.Application;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBankingApi.Controllers;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
public abstract class BaseApiController : ControllerBase
{

    protected IActionResult BadRequest400WithErrorMessagesFromResult(Result result)
    {
        // Este metodo solamente se usa en caso de que el Result haya fallado, es decir, isFailure
        if (!string.IsNullOrEmpty(result.GeneralError))
        {
            return BadRequest(result.GeneralError);
        }
        else
        {
            return BadRequest(result.Errors);
        }
    }
}