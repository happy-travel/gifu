using System.Text.Json.Serialization;

namespace HappyTravel.Gifu.Api.Models.Ixaris.Response;

public readonly struct IxarisData
{
    [JsonPropertyName("transactionReference")]
    public string TransactionReference { get; init; }

    [JsonPropertyName("scheduleReference")]
    public string ScheduleReference { get; init; }
}