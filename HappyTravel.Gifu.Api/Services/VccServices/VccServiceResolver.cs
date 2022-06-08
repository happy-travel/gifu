using CSharpFunctionalExtensions;
using HappyTravel.Gifu.Api.Infrastructure.Logging;
using HappyTravel.Gifu.Api.Infrastructure.Options;
using HappyTravel.Gifu.Api.Models;
using HappyTravel.Gifu.Data;
using HappyTravel.Money.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HappyTravel.Gifu.Api.Services.VccServices;

public class VccServiceResolver
{
    public VccServiceResolver(ILogger<VccServiceResolver> logger, IOptions<VccServiceResolverOptions> options, 
        IEnumerable<IVccSupplierService> vccServices)
    {
        _logger = logger;
        _options = options.Value;
        _vccServices = vccServices;
    }


    public Result<IVccSupplierService> ResolveService(List<CreditCardTypes>? types, Currencies currency)
    {
        if (types is null)
            return ResolveServiceByCurrency(currency);

        if (_options.AmexCurrencies.Contains(currency) && _options.AmexCreditCardTypes.Any(type => types.Contains(type)))
            return GetService(typeof(AmExService));

        if (_options.IxarisCurrencies.Contains(currency) && _options.IxarisCreditCardTypes.Any(type => types.Contains(type)))
            return GetService(typeof(IxarisService));

        _logger.LogVccServiceResolveFailure();
        return Result.Failure<IVccSupplierService>($"Unable to resolve a vccService for currency `{currency}` and VccVendors `{string.Join(", ", types)}`");
    }


    public Result<IVccSupplierService> ResolveServiceByCurrency(Currencies currency)
    {
        if (_options.AmexCurrencies.Contains(currency))
            return GetService(typeof(AmExService));

        if (_options.IxarisCurrencies.Contains(currency))
            return GetService(typeof(IxarisService));

        _logger.LogVccServiceResolveFailure();
        return Result.Failure<IVccSupplierService>($"Unable to resolve a vccService for currency `{currency}`");
    }


    public Result<IVccSupplierService> ResolveServiceByVccVendor(VccVendors vccVendor)
    {
        switch (vccVendor)
        {
            case VccVendors.AmericanExpress:
                return GetService(typeof(AmExService));
            case VccVendors.Ixaris:
                return GetService(typeof(IxarisService));
            default:
                _logger.LogVccServiceResolveFailure();
                return Result.Failure<IVccSupplierService>($"Unable to resolve a vccService for VccVendor `{vccVendor}`");
        }
    }


    private Result<IVccSupplierService> GetService(Type type)
    {
        _logger.LogVccServiceResolveSuccess(type.Name);
        return Result.Success(_vccServices.Single(s => s.GetType() == type));
    }


    private readonly ILogger<VccServiceResolver> _logger;
    private readonly VccServiceResolverOptions _options;
    private readonly IEnumerable<IVccSupplierService> _vccServices;
}