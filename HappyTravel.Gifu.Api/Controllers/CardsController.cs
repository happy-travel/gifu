using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Gifu.Api.Models;
using HappyTravel.Gifu.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Gifu.Api.Controllers
{
    [ApiController]
    [Authorize(Policy = "CanCreateCard")]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/cards")]
    public class CardsController : ControllerBase
    {
        public CardsController(IVccService vccService, IClientService clientService)
        {
            _vccService = vccService;
            _clientService = clientService;
        }
        
        
        /// <summary>
        /// Issues new virtual credit card
        /// </summary>
        /// <param name="request">Vcc request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Virtual credit card info</returns>
        [HttpPost]
        [ProducesResponseType(typeof(VirtualCreditCard), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Issue(VccIssueRequest request, CancellationToken cancellationToken)
        {
            var (_, isFailure, clientId, error) = _clientService.GetId();
            if (isFailure)
                return BadRequest(new ProblemDetails { Detail = error });

            var info = await _vccService.Issue(request, clientId, cancellationToken);
            return info.IsSuccess
                ? Ok(info.Value)
                : BadRequest(new ProblemDetails { Detail = info.Error });
        }


        private readonly IVccService _vccService;
        private readonly IClientService _clientService;
    }
}