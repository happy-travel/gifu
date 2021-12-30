using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HappyTravel.Gifu.Api.Models.AmEx.Request;

public struct ReconciliationFields
{
    [JsonPropertyName("user_defined_fields_group")]
    public List<CustomField> UserDefinedFieldsGroup { get; init; }
}