using System;

namespace HappyTravel.Gifu.Data.Models;

public class IxarisScheduleLoad
{
    public int Id { get; set; }
    public string ScheduleReference { get; set; } = string.Empty;
    public string CardReference { get; set; } = string.Empty;
    public DateTime Created { get; set; }
    public DateTime Modified { get; set; }
    public IxarisScheduleLoadStatuses Status { get; set; }
}