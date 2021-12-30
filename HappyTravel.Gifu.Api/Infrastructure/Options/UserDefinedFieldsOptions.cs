using System.Collections.Generic;

namespace HappyTravel.Gifu.Api.Infrastructure.Options;

public class UserDefinedFieldsOptions
{
    public AmexFieldSettings BookingReferenceCode { get; set; } = new();
    public Dictionary<string, AmexFieldSettings> CustomFields { get; set; } = new();
}