using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Gifu.Api.Models;
using HappyTravel.Gifu.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Gifu.Api.Controllers
{
    [ApiController]
    [Authorize(Policy = "CanEdit")]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/cards/")]
    public class CardsEditController : ControllerBase
    {
        public CardsEditController(IClientService clientService, IVccService vccService)
        {
            _clientService = clientService;
            _vccService = vccService;
        }


        [HttpPost("{referenceCode}/edit")]
        public async Task<IActionResult> EditCard(string referenceCode, [FromBody] VccEditRequest request)
        {
            var (_, isFailure, clientId, error) = _clientService.GetId();
            if (isFailure)
                return BadRequest(new ProblemDetails { Detail = error });

            var (isSuccess, _, editError) = await _vccService.Edit(referenceCode, request, clientId);
            return isSuccess
                ? Ok()
                : BadRequest(new ProblemDetails { Detail =  editError});
        }
        
        
        private readonly IClientService _clientService;
        private readonly IVccService _vccService;
    }
}