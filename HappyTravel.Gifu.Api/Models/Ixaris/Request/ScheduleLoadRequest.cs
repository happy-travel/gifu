using System.Text.Json.Serialization;

namespace HappyTravel.Gifu.Api.Models.Ixaris.Request
{
    public readonly struct ScheduleLoadRequest
    {
        [JsonPropertyName("cardReference")]
        public string CardReference { get; init; }

        [JsonPropertyName("fundingAccountReference")]
        public string FundingAccountReference { get; init; }

        [JsonPropertyName("amount")]
        public decimal Amount { get; init; }

        [JsonPropertyName("scheduleDate")]
        public string ScheduleDate { get; init; }

        [JsonPropertyName("clearanceDate")]
        public string ClearanceDate { get; init; }
    }
}
