using System;

namespace HappyTravel.Gifu.Data.Models
{
    public class AmountChangesHistory
    {
        public string VccId { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal AmountBefore { get; set; }
        public decimal AmountAfter { get; set; }
    }
}