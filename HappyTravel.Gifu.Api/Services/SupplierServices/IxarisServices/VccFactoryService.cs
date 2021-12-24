using CSharpFunctionalExtensions;
using HappyTravel.Gifu.Api.Infrastructure.Options;
using HappyTravel.Gifu.Api.Models;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;

namespace HappyTravel.Gifu.Api.Services.SupplierServices.IxarisServices
{
    public class VccFactoryService : IVccFactoryService
    {
        public VccFactoryService(IOptions<IxarisOptions> options)
        {
            _options = options.Value;
        }


        public Result<KeyValuePair<CreditCardTypes, string>> GetVccFactory(List<CreditCardTypes>? types)
        {
            if (types is null || types.Contains(_options.DefaultVccType))
                return _options.VccFactoryNames.First(f => f.Key == _options.DefaultVccType);

            var vccFactory = _options.VccFactoryNames.FirstOrDefault(f => types.Contains(f.Key));

            if (!vccFactory.Equals(default(KeyValuePair<CreditCardTypes, string>)))
                return vccFactory;
            else
                return Result.Failure<KeyValuePair<CreditCardTypes, string>>($"Cannot get vccFactoryName for VCC vendors `{string.Join(", ", types)}`");
        }


        private readonly IxarisOptions _options;
    }
}
