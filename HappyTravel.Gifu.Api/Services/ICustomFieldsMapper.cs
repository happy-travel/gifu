using System.Collections.Generic;
using HappyTravel.Gifu.Api.Models.AmEx.Request;

namespace HappyTravel.Gifu.Api.Services
{
    public interface ICustomFieldsMapper
    {
        List<CustomField> Map(string referenceCode, Dictionary<string, string?> dictionary);
    }
}