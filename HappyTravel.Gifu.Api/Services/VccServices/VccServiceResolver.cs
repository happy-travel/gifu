using CSharpFunctionalExtensions;
using HappyTravel.Gifu.Api.Infrastructure.Options;
using HappyTravel.Gifu.Data;
using HappyTravel.Money.Enums;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HappyTravel.Gifu.Api.Services.VccServices
{
    public class VccServiceResolver
    {
        public VccServiceResolver(IOptions<VccServiceResolverOptions> options, IEnumerable<IVccSupplierService> vccServices)
        {
            _options = options.Value;
            _vccServices = vccServices;
        }


        public Result<IVccSupplierService> ResolveServiceByCurrency(Currencies currency)
        {
            if (_options.AmexCurrencies.Contains(currency))
                return GetService(typeof(AmExService));

            return Result.Failure<IVccSupplierService>(string.Format($"Unable to issue VCC for currency `{currency}`"));
        }


        public Result<IVccSupplierService> ResolveServiceBySupplierCode(VccSuppliers supplierCode)
            => supplierCode switch
            {
                VccSuppliers.AmEx => GetService(typeof(AmExService)),                
                _ => Result.Failure<IVccSupplierService>(string.Format($"Unable to issue VCC for supplier `{supplierCode}`"))
            };


        private Result<IVccSupplierService> GetService(Type type)
            => Result.Success(_vccServices.Single(s => s.GetType() == type));
        

        private readonly VccServiceResolverOptions _options;
        private readonly IEnumerable<IVccSupplierService> _vccServices;
    }
}
