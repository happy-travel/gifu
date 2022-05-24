namespace HappyTravel.Gifu.Api.Models.Ixaris.Request;

public readonly struct UpdateScheduleLoadRequest
{
    public string FundingAccountReference { get; init; }

    public decimal Amount { get; init; }

    public string ScheduleDate { get; init; }

    public string ClearanceDate { get; init; }
}