using System.Collections.Generic;

namespace HappyTravel.Gifu.Api.Services.SupplierServices.IxarisServices;

public interface IIxarisCardInfoMapper
{
    List<Dictionary<string, string>> Map(string referenceCode, Dictionary<string, string?>? specialValues);
}
