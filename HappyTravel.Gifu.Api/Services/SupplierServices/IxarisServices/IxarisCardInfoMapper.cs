using HappyTravel.Gifu.Api.Infrastructure.Options;
using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace HappyTravel.Gifu.Api.Services.SupplierServices.IxarisServices;

public class IxarisCardInfoMapper : IIxarisCardInfoMapper
{
    public IxarisCardInfoMapper(IOptionsMonitor<IxarisUserDefinedFieldsOptions> fieldOptionsMonitor)
    {
        _fieldOptionsMonitor = fieldOptionsMonitor;
    }


    public List<Dictionary<string, string>> Map(string referenceCode, Dictionary<string, string?>? specialValues)
    {
        var fieldsOptions = _fieldOptionsMonitor.CurrentValue;

        var cardInfo = new List<Dictionary<string, string>>()
        {            
            new ()
            {
                { "Confirmation number", referenceCode }
            }
        };        

        if (specialValues is null)
            return cardInfo;

        foreach (var (key, value) in specialValues)
        {
            if (value is null)
                continue;

            if (fieldsOptions.CardInfoMapping.TryGetValue(key, out var ixarisKey))
                cardInfo.Add(new()
                {
                    { ixarisKey, value }
                });
        }

        return cardInfo;
    }


    private readonly IOptionsMonitor<IxarisUserDefinedFieldsOptions> _fieldOptionsMonitor;
}
