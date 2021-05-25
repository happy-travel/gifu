using System;
using System.ComponentModel.DataAnnotations;

namespace HappyTravel.Gifu.Api.Models
{
    public readonly struct VccInfo
    {
        [Required]
        public string Number { get; init; }
        
        [Required]
        public DateTime Expiry { get; init; }
        
        [Required]
        public string Holder { get; init; }
        
        [Required]
        public string Code { get; init; }
    }
}