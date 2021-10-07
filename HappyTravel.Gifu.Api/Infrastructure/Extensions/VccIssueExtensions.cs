using System.Collections.Generic;
using System.Linq;
using HappyTravel.Gifu.Data.Models;

namespace HappyTravel.Gifu.Api.Infrastructure.Extensions
{
    public static class VccIssueExtensions
    {
        public static IEnumerable<VccIssue> TrimCardNumbers(this List<VccIssue> vccIssues)
            => vccIssues.Select(v => v.TrimCardNumber());
        
        
        private static VccIssue TrimCardNumber(this VccIssue vccIssue)
        {
            vccIssue.CardNumber = TrimCardNumber(vccIssue.CardNumber);
            return vccIssue;
        }


        private static string TrimCardNumber(string cardNumber)
        {
            if (string.IsNullOrEmpty(cardNumber))
                return cardNumber;

            var cardNumberLength = cardNumber.Length;
            return cardNumber[^4..].PadLeft(cardNumberLength - 4, '*');
        }
    }
}