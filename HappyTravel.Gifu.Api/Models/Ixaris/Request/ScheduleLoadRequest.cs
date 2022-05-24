namespace HappyTravel.Gifu.Api.Models.Ixaris.Request;

public readonly struct ScheduleLoadRequest
{
    public string CardReference { get; init; }

    public string FundingAccountReference { get; init; }

    public decimal Amount { get; init; }

    public string ScheduleDate { get; init; }

    public string ClearanceDate { get; init; }
}