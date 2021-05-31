using System.Security.Claims;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http;

namespace HappyTravel.Gifu.Api.Services
{
    public class ClientService : IClientService
    {
        public ClientService(IHttpContextAccessor context)
        {
            _context = context;
        }
        
        
        public Result<string> GetId()
        {
            var clientId = _context.HttpContext?.User.FindFirst("client_id")?.Value;

            return string.IsNullOrEmpty(clientId)
                ? Result.Failure<string>("Failed to get client id")
                : clientId;
        }


        private readonly IHttpContextAccessor _context;
    }
}