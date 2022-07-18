using System.Collections.Generic;

namespace HappyTravel.Gifu.Api.Infrastructure.Options;

public class IxarisUserDefinedFieldsOptions
{
    public Dictionary<string, string> CardInfoMapping { get; set; } = new();
}
