using CSharpFunctionalExtensions;
using HappyTravel.Gifu.Api.Infrastructure.Options;
using HappyTravel.Gifu.Api.Models;
using HappyTravel.Gifu.Data;
using HappyTravel.Money.Enums;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HappyTravel.Gifu.Api.Services.VccServices;

public class VccServiceResolver
{
    public VccServiceResolver(IOptions<VccServiceResolverOptions> options, IEnumerable<IVccSupplierService> vccServices)
    {
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

        return Result.Failure<IVccSupplierService>($"Unable to issue a VCC for currency `{currency}` and VccVendors `{string.Join(", ", types)}`");
    }


    public Result<IVccSupplierService> ResolveServiceByCurrency(Currencies currency)
    {
        if (_options.AmexCurrencies.Contains(currency))
            return GetService(typeof(AmExService));

        if (_options.IxarisCurrencies.Contains(currency))
            return GetService(typeof(IxarisService));

        return Result.Failure<IVccSupplierService>($"Unable to issue a VCC for currency `{currency}`");
    }


    public Result<IVccSupplierService> ResolveServiceByVccVendor(VccVendors vccVendor)
        => vccVendor switch
        {
            VccVendors.AmericanExpress => GetService(typeof(AmExService)),
            VccVendors.Ixaris => GetService(typeof(IxarisService)),
            _ => Result.Failure<IVccSupplierService>($"Unable to issue a VCC for VccVendor `{vccVendor}`")
        };


    private Result<IVccSupplierService> GetService(Type type)
        => Result.Success(_vccServices.Single(s => s.GetType() == type));
        

    private readonly VccServiceResolverOptions _options;
    private readonly IEnumerable<IVccSupplierService> _vccServices;
}