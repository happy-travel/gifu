using System;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Gifu.Api.Infrastructure.Logging
{
    public static class LoggerExtensions
    {
        static LoggerExtensions()
        {
            ClientIdRetrievalFailure = LoggerMessage.Define(LogLevel.Warning,
                new EventId(81000, "ClientIdRetrievalFailure"),
                "Could not get client id for authenticated user");
            
            VccIssueRequestStarted = LoggerMessage.Define<string, decimal, string>(LogLevel.Information,
                new EventId(81010, "VccIssueRequestStarted"),
                "Processing VCC issue request for '{ReferenceCode}'. Amount: {Amount} {Currency},");
            
            VccIssueRequestFailure = LoggerMessage.Define<string, string>(LogLevel.Error,
                new EventId(81015, "VccIssueRequestFailure"),
                "Processing VCC issue request for '{ReferenceCode}' failed: '{Error}'");
            
            VccIssueRequestSuccess = LoggerMessage.Define<string, string>(LogLevel.Information,
                new EventId(81016, "VccIssueRequestSuccess"),
                "Processing VCC issue request for '{ReferenceCode}' completed successfully. UniqueId {UniqueId}");
            
        }
    
                
         public static void LogClientIdRetrievalFailure(this ILogger logger, Exception exception = null)
            => ClientIdRetrievalFailure(logger, exception);
                
         public static void LogVccIssueRequestStarted(this ILogger logger, string ReferenceCode, decimal Amount, string Currency, Exception exception = null)
            => VccIssueRequestStarted(logger, ReferenceCode, Amount, Currency, exception);
                
         public static void LogVccIssueRequestFailure(this ILogger logger, string ReferenceCode, string Error, Exception exception = null)
            => VccIssueRequestFailure(logger, ReferenceCode, Error, exception);
                
         public static void LogVccIssueRequestSuccess(this ILogger logger, string ReferenceCode, string UniqueId, Exception exception = null)
            => VccIssueRequestSuccess(logger, ReferenceCode, UniqueId, exception);
    
    
        
        private static readonly Action<ILogger, Exception> ClientIdRetrievalFailure;
        
        private static readonly Action<ILogger, string, decimal, string, Exception> VccIssueRequestStarted;
        
        private static readonly Action<ILogger, string, string, Exception> VccIssueRequestFailure;
        
        private static readonly Action<ILogger, string, string, Exception> VccIssueRequestSuccess;
    }
}