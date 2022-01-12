using System;
using System.Collections.Generic;
using System.Linq;
using HappyTravel.Gifu.Api.Infrastructure.Options;
using HappyTravel.Gifu.Api.Models.AmEx.Request;
using Microsoft.Extensions.Options;

namespace HappyTravel.Gifu.Api.Services;

public class CustomFieldsMapper : ICustomFieldsMapper
{
    public CustomFieldsMapper(IOptionsMonitor<UserDefinedFieldsOptions> fieldOptionsMonitor)
    {
        _fieldOptionsMonitor = fieldOptionsMonitor;
    }
        
        
    public List<CustomField> Map(string referenceCode, Dictionary<string, string?>? dictionary)
    {
        var fieldsOptions = _fieldOptionsMonitor.CurrentValue;
            
        var list = new List<CustomField>
        {
            new()
            {
                Index = fieldsOptions.BookingReferenceCode.Index,
                Value = referenceCode[..Math.Min(fieldsOptions.BookingReferenceCode.Length, referenceCode.Length)]
            }
        };

        if (dictionary == null) 
            return list;
        
        foreach (var (key, value) in dictionary)
        {
            if (value is null)
                continue;

            if (fieldsOptions.CustomFields.TryGetValue(key, out var fieldSettings))
            {
                list.Add(new CustomField
                {
                    Index = fieldSettings.Index,
                    Value = value[..Math.Min(fieldSettings.Length, value.Length)]
                });
            }
        }

        return list;
    }
        
        
    private readonly IOptionsMonitor<UserDefinedFieldsOptions> _fieldOptionsMonitor;
}