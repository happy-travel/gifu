using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HappyTravel.Gifu.Api.Services;
using HappyTravel.Gifu.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Gifu.Api.Controllers
{
    [ApiController]
    [Authorize(Policy = "CanGetReport")]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/history")]
    public class HistoryController : ControllerBase
    {
        public HistoryController(IVccService vccService)
        {
            _vccService = vccService;
        }


        /// <summary>
        /// Returns list of generated cards
        /// </summary>
        /// <param name="referenceCodes">Booking reference codes</param>
        /// <param name="cancellationToken">Cancellation token</param>
        [HttpPost]
        [ProducesResponseType(typeof(List<VccIssue>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCardsInfo([FromBody] List<string> referenceCodes, CancellationToken cancellationToken)
        {
            return Ok(await _vccService.GetCardsInfo(referenceCodes, cancellationToken));
        }
        
        
        private readonly IVccService _vccService;
    }
}