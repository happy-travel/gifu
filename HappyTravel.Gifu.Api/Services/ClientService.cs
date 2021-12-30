using CSharpFunctionalExtensions;
using HappyTravel.Gifu.Api.Infrastructure.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Gifu.Api.Services;

public class ClientService : IClientService
{
    public ClientService(IHttpContextAccessor context, ILogger<ClientService> logger)
    {
        _context = context;
        _logger = logger;
    }
        
        
    public Result<string> GetId()
    {
        var clientId = _context.HttpContext?.User.FindFirst("client_id")?.Value;
        if (string.IsNullOrEmpty(clientId))
        {
            _logger.LogClientIdRetrievalFailure();
            return Result.Failure<string>("Failed to get client id");
        }

        return clientId;
    }


    private readonly IHttpContextAccessor _context;
    private readonly ILogger<ClientService> _logger;
}