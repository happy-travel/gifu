using System;

namespace HappyTravel.Gifu.Data.Models
{
    public class VccEditLog
    {
        public int Id { get; set; }
        public string VccId { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
        public DateTime Created { get; set; }
    }
}