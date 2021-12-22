using CSharpFunctionalExtensions;
using HappyTravel.Gifu.Api.Infrastructure.Options;
using HappyTravel.Gifu.Api.Models;
using Microsoft.Extensions.Options;

namespace HappyTravel.Gifu.Api.Services.SupplierServices.IxarisServices
{
    public class VccFactoryNameService : IVccFactoryNameService
    {
        public VccFactoryNameService(IOptions<IxarisOptions> options)
        {
            _options = options.Value;
        }


        public Result<string> GetVccFactoryName(CreditCardTypes? type)
        {
            return type is null
                ? _options.VccFactoryNames[_options.DefaultVccType]
                : _options.VccFactoryNames.TryGetValue(type.Value, out var vccFactoryName)
                ? vccFactoryName
                : Result.Failure<string>($"Cannot get vccFactoryName for VCC vendor `{type}`");
        }


        private readonly IxarisOptions _options;
    }
}
