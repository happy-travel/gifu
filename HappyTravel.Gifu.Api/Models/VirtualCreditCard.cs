using System;

namespace HappyTravel.Gifu.Api.Models
{
    public readonly struct VirtualCreditCard
    {
        public VirtualCreditCard(string number, DateTime expiry, string holder, string code)
        {
            Number = number;
            Expiry = expiry;
            Holder = holder;
            Code = code;
        }
        
        
        public string Number { get; }
        public DateTime Expiry { get; }
        public string Holder { get; }
        public string Code { get; }
    }
}