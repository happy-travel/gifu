using System;

namespace HappyTravel.Gifu.Data.Models;

public class IxarisScheduleLoad
{
    public int Id { get; set; }
    public string ScheduleReference { get; set; } = string.Empty;
    public string CardReference { get; set; } = string.Empty;
    public DateTimeOffset ScheduleDate { get; set; }
    public DateTimeOffset ClearanceDate { get; set; }
    public DateTimeOffset Created { get; set; }
    public DateTimeOffset Modified { get; set; }
    public IxarisScheduleLoadStatuses Status { get; set; }
}