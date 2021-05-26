﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentValidation;
using HappyTravel.Gifu.Api.Models;
using HappyTravel.Money.Enums;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Gifu.Api.Services
{
    public class VccService : IVccService
    {
        public VccService(IAmExClient client, ILogger<VccService> logger)
        {
            _client = client;
            _logger = logger;
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


            Task<Result<Vcc>> CreateCard()
                => _client.CreateCard(request.ReferenceCode, request.MoneyAmount, request.DueDate);

            
            Result<Vcc> WriteLog(Result<Vcc> result)
            {
                if (result.IsFailure)
                    _logger.LogError("Creating VCC for reference code `{ReferenceCode}` completed with error `{Error}`", request.ReferenceCode, result.Error);
                else
                    _logger.LogInformation("Creating Vcc for reference code `{ReferenceCode}` completed", request.ReferenceCode);

                return result;
            }
        }
        

        private readonly IAmExClient _client;
        private readonly ILogger<VccService> _logger;
    }
}