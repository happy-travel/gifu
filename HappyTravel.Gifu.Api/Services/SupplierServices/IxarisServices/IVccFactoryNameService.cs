using CSharpFunctionalExtensions;
using HappyTravel.Gifu.Api.Models;

namespace HappyTravel.Gifu.Api.Services.SupplierServices.IxarisServices
{
    public interface IVccFactoryNameService
    {
        Result<string> GetVccFactoryName(CreditCardTypes? type);
    }
}
