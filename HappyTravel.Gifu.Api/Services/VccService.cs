using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentValidation;
using HappyTravel.Gifu.Api.Infrastructure.Extensions;
using HappyTravel.Gifu.Api.Models;
using HappyTravel.Gifu.Data;
using HappyTravel.Gifu.Data.Models;
using HappyTravel.Money.Enums;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Gifu.Api.Services
{
    public class VccService : IVccService
    {
        public VccService(IAmExClient client, ILogger<VccService> logger, GifuContext context)
        {
            _client = client;
            _logger = logger;
            _context = context;
        }
        
        
        public Task<Result<Vcc>> Issue(VccIssueRequest request, CancellationToken cancellationToken)
        {
            return ValidateRequest(request)
                .Bind(CreateCard)
                .Finally(WriteLog);


            static Result ValidateRequest(VccIssueRequest request)
            {
                var validator = new InlineValidator<VccIssueRequest>();

                validator.RuleFor(r => r.DueDate.Date).GreaterThan(DateTime.UtcNow.Date);
                validator.RuleFor(r => r.MoneyAmount.Currency).Equal(Currencies.USD);
                validator.RuleFor(r => r.MoneyAmount.Amount).GreaterThan(0);
                validator.RuleFor(r => r.ReferenceCode).NotEmpty();

                var result = validator.Validate(request);

                return result.IsValid
                    ? Result.Success()
                    : Result.Failure(string.Join(";", result.Errors.Select(e => e.ErrorMessage)));
            }


            async Task<Result<(string, Vcc)>> CreateCard()
            {
                var (transactionId, response) = await _client.CreateToken(request.ReferenceCode, request.MoneyAmount, request.DueDate);
                return response.Status.ShortMessage != "success" 
                    ? Result.Failure<(string, Vcc)>(response.Status.DetailedMessage) 
                    : (transactionId, response.TokenIssuanceData.TokenDetails.ToVcc());
            }

            
            async Task<Result<Vcc>> WriteLog(Result<(string TransactionId, Vcc Vcc)> result)
            {
                if (result.IsFailure)
                {
                    _logger.LogError("Creating VCC for reference code `{ReferenceCode}` completed with error `{Error}`", request.ReferenceCode, result.Error);
                    return Result.Failure<Vcc>($"Error creating VCC for reference code `{request.ReferenceCode}`");
                }
                
                _context.Issues.Add(new Issue
                {
                    TransactionId = result.Value.TransactionId,
                    ReferenceCode = request.ReferenceCode,
                    Amount = request.MoneyAmount.Amount,
                    Currency = request.MoneyAmount.Currency,
                    DueDate = request.DueDate
                });
                
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Creating Vcc for reference code `{ReferenceCode}` completed", request.ReferenceCode);
                
                return result.Value.Vcc;
            }
        }
        

        private readonly IAmExClient _client;
        private readonly ILogger<VccService> _logger;
        private readonly GifuContext _context;
    }
}