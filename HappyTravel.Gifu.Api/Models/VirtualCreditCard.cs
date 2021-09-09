using System;

namespace HappyTravel.Gifu.Api.Models
{
    public readonly struct VirtualCreditCard
    {
        public VirtualCreditCard(string number, DateTime expiry, string holder, string code, CreditCardTypes type)
        {
            Number = number;
            Expiry = expiry;
            Holder = holder;
            Code = code;
            Type = type;
        }
        
        
        public string Number { get; }
        public DateTime Expiry { get; }
        public string Holder { get; }
        public string Code { get; }
        public CreditCardTypes Type { get; }
    }
}