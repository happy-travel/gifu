using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Gifu.Api.Models;
using HappyTravel.Gifu.Data;

namespace HappyTravel.Gifu.Api.Services
{
    public class VccIssueService : IVccIssueService
    {
        public VccIssueService(IHttpClientFactory clientFactory, GifuContext context)
        {
            _clientFactory = clientFactory;
            _context = context;
        }
        
        
        public Task<Result<VccInfo>> Issue(VccIssueRequest request, CancellationToken cancellationToken)
        {
            var client = _clientFactory.CreateClient(HttpClientName);
            
            return Task.FromResult(Result.Failure<VccInfo>("Not implemented"));
        }

        
        public const string HttpClientName = "AmExHttpClient";
        

        private readonly IHttpClientFactory _clientFactory;
        private readonly GifuContext _context;
    }
}