using System;
using System.Collections.Generic;
using HappyTravel.Gifu.Api.Infrastructure.Extensions;
using HappyTravel.Gifu.Api.Models.AmEx.Request;
using HappyTravel.Money.Models;

namespace HappyTravel.Gifu.Api.Infrastructure.Utils;

public static class RequestGenerator
{
    public static CreateTokenRequest GenerateCreateTokenRequest(string uniqueId, string accountId, MoneyAmount amount, 
        DateTimeOffset startDate, DateTimeOffset endDate, List<CustomField> customFields)
    {
        return new CreateTokenRequest
        {
            TokenIssuanceParams = new TokenIssuanceParams
            {
                BillingAccountId = accountId,
                TokenDetails = new TokenDetails
                {
                    TokenReferenceId = uniqueId,
                    TokenAmount = amount.ToAmExFormat(),
                    TokenStartDate = startDate.ToAmExFormat(),
                    TokenEndDate = endDate.ToAmExFormat()
                },
                ReconciliationFields = new ReconciliationFields
                {
                    UserDefinedFieldsGroup = customFields
                }
            }
        };
    }
        
        
    public static ModifyRequest GenerateModifyTokenRequest(string tokenNumber, string accountId, MoneyAmount? tokenAmount, 
        DateTimeOffset? tokenStartDate, DateTimeOffset? tokenDueDate)
    {
        return new ModifyRequest
        {
            TokenIdentifier = new TokenIdentifier
            {
                TokenNumber  = tokenNumber
            },
            TokenIssuanceParams = new TokenIssuanceParams
            {
                BillingAccountId = accountId,
                TokenDetails = new TokenDetails
                {
                    TokenAmount = tokenAmount?.ToAmExFormat(),
                    TokenStartDate = tokenStartDate?.ToAmExFormat(),
                    TokenEndDate = tokenDueDate?.ToAmExFormat()
                }
            }
        };
    }
}