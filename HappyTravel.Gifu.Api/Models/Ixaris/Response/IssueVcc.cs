using System.Text.Json.Serialization;

namespace HappyTravel.Gifu.Api.Models.Ixaris.Response;

public class IssueVcc
{
    [JsonPropertyName("cardReference")]
    public string CardReference { get; init; }

    [JsonPropertyName("transactionReference")]
    public string TransactionReference { get; init; }
}