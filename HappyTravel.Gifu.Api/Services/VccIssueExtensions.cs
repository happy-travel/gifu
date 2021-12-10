using System.Collections.Generic;
using System.Linq;
using HappyTravel.Gifu.Data.Models;

namespace HappyTravel.Gifu.Api.Services
{
    public static class VccIssueExtensions
    {
        public static IEnumerable<VccIssue> MaskCardNumbers(this List<VccIssue> vccIssues)
            => vccIssues.Select(v => v.MaskCardNumber());
        
        
        private static VccIssue MaskCardNumber(this VccIssue vccIssue)
        {
            vccIssue.CardNumber = MaskCardNumber(vccIssue.CardNumber);
            return vccIssue;
        }


        private static string MaskCardNumber(string cardNumber)
        {
            if (string.IsNullOrEmpty(cardNumber))
                return cardNumber;

            var cardNumberLength = cardNumber.Length;
            return cardNumber[^4..].PadLeft(cardNumberLength - 4, '*');
        }
    }
}