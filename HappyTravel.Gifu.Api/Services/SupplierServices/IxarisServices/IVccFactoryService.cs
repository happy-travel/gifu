using CSharpFunctionalExtensions;
using HappyTravel.Gifu.Api.Models;
using System.Collections.Generic;

namespace HappyTravel.Gifu.Api.Services.SupplierServices.IxarisServices
{
    public interface IVccFactoryService
    {
        Result<KeyValuePair<CreditCardTypes, string>> GetVccFactory(List<CreditCardTypes>? types);
    }
}
