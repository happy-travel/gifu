using System.Threading;
using System.Threading.Tasks;
using HappyTravel.Gifu.Api.Models;
using HappyTravel.Gifu.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Gifu.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/issues")]
    public class IssuesController : ControllerBase
    {
        public IssuesController(IVccService vccService)
        {
            _vccService = vccService;
        }
        
        
        /// <summary>
        /// Issues new virtual credit card
        /// </summary>
        /// <param name="request">Vcc request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Virtual credit card info</returns>
        [HttpPost]
        [ProducesResponseType(typeof(Vcc), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Issue(VccIssueRequest request, CancellationToken cancellationToken)
        {
            var info = await _vccService.Issue(request, cancellationToken);
            return info.IsSuccess
                ? Ok(info.Value)
                : BadRequest(new ProblemDetails { Detail = info.Error });
        }


        private readonly IVccService _vccService;
    }
}