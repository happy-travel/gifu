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
                "Processing VCC issue request for '{ReferenceCode}' completed successfully. UniqueId: '{UniqueId}'");
            
            VccDeleteRequestStarted = LoggerMessage.Define<string>(LogLevel.Information,
                new EventId(81020, "VccDeleteRequestStarted"),
                "Deleting VCC for '{ReferenceCode}'");
            
            VccDeleteRequestFailure = LoggerMessage.Define<string, string>(LogLevel.Error,
                new EventId(81021, "VccDeleteRequestFailure"),
                "Deleting VCC for '{ReferenceCode}' failed. '{Error}'");
            
            VccDeleteRequestSuccess = LoggerMessage.Define<string>(LogLevel.Information,
                new EventId(81022, "VccDeleteRequestSuccess"),
                "Deleting VCC for '{ReferenceCode}' completed successfully");
            
            VccModifyAmountRequestStarted = LoggerMessage.Define<string, decimal>(LogLevel.Information,
                new EventId(81030, "VccModifyAmountRequestStarted"),
                "Modifying VCC amount for '{ReferenceCode}'. New value: {amount}");
            
            VccModifyAmountRequestFailure = LoggerMessage.Define<string, string>(LogLevel.Error,
                new EventId(81031, "VccModifyAmountRequestFailure"),
                "Modifying VCC amount for '{ReferenceCode}' failed. '{Error}'");
            
            VccModifyAmountRequestSuccess = LoggerMessage.Define<string, decimal>(LogLevel.Information,
                new EventId(81032, "VccModifyAmountRequestSuccess"),
                "Modifying VCC amount for '{ReferenceCode}' completed successfully. New value: {amount}");
            
            ResponseDeserializationFailed = LoggerMessage.Define<string>(LogLevel.Error,
                new EventId(81040, "ResponseDeserializationFailed"),
                "Response deserialization failed: {Response}");
            
            VccEditRequestStarted = LoggerMessage.Define<string>(LogLevel.Information,
                new EventId(81050, "VccEditRequestStarted"),
                "Editing VCC amount for '{ReferenceCode}' started");
            
            VccEditFailure = LoggerMessage.Define<string, string>(LogLevel.Error,
                new EventId(81051, "VccEditFailure"),
                "Editing VCC for '{ReferenceCode}' failed. '{Error}'");
            
            VccEditSuccess = LoggerMessage.Define<string>(LogLevel.Information,
                new EventId(81052, "VccEditSuccess"),
                "Editing VCC for '{ReferenceCode}' completed successfully");
            
        }
    
                
         public static void LogClientIdRetrievalFailure(this ILogger logger, Exception exception = null)
            => ClientIdRetrievalFailure(logger, exception);
                
         public static void LogVccIssueRequestStarted(this ILogger logger, string ReferenceCode, decimal Amount, string Currency, Exception exception = null)
            => VccIssueRequestStarted(logger, ReferenceCode, Amount, Currency, exception);
                
         public static void LogVccIssueRequestFailure(this ILogger logger, string ReferenceCode, string Error, Exception exception = null)
            => VccIssueRequestFailure(logger, ReferenceCode, Error, exception);
                
         public static void LogVccIssueRequestSuccess(this ILogger logger, string ReferenceCode, string UniqueId, Exception exception = null)
            => VccIssueRequestSuccess(logger, ReferenceCode, UniqueId, exception);
                
         public static void LogVccDeleteRequestStarted(this ILogger logger, string ReferenceCode, Exception exception = null)
            => VccDeleteRequestStarted(logger, ReferenceCode, exception);
                
         public static void LogVccDeleteRequestFailure(this ILogger logger, string ReferenceCode, string Error, Exception exception = null)
            => VccDeleteRequestFailure(logger, ReferenceCode, Error, exception);
                
         public static void LogVccDeleteRequestSuccess(this ILogger logger, string ReferenceCode, Exception exception = null)
            => VccDeleteRequestSuccess(logger, ReferenceCode, exception);
                
         public static void LogVccModifyAmountRequestStarted(this ILogger logger, string ReferenceCode, decimal amount, Exception exception = null)
            => VccModifyAmountRequestStarted(logger, ReferenceCode, amount, exception);
                
         public static void LogVccModifyAmountRequestFailure(this ILogger logger, string ReferenceCode, string Error, Exception exception = null)
            => VccModifyAmountRequestFailure(logger, ReferenceCode, Error, exception);
                
         public static void LogVccModifyAmountRequestSuccess(this ILogger logger, string ReferenceCode, decimal amount, Exception exception = null)
            => VccModifyAmountRequestSuccess(logger, ReferenceCode, amount, exception);
                
         public static void LogResponseDeserializationFailed(this ILogger logger, string Response, Exception exception = null)
            => ResponseDeserializationFailed(logger, Response, exception);
                
         public static void LogVccEditRequestStarted(this ILogger logger, string ReferenceCode, Exception exception = null)
            => VccEditRequestStarted(logger, ReferenceCode, exception);
                
         public static void LogVccEditFailure(this ILogger logger, string ReferenceCode, string Error, Exception exception = null)
            => VccEditFailure(logger, ReferenceCode, Error, exception);
                
         public static void LogVccEditSuccess(this ILogger logger, string ReferenceCode, Exception exception = null)
            => VccEditSuccess(logger, ReferenceCode, exception);
    
    
        
        private static readonly Action<ILogger, Exception> ClientIdRetrievalFailure;
        
        private static readonly Action<ILogger, string, decimal, string, Exception> VccIssueRequestStarted;
        
        private static readonly Action<ILogger, string, string, Exception> VccIssueRequestFailure;
        
        private static readonly Action<ILogger, string, string, Exception> VccIssueRequestSuccess;
        
        private static readonly Action<ILogger, string, Exception> VccDeleteRequestStarted;
        
        private static readonly Action<ILogger, string, string, Exception> VccDeleteRequestFailure;
        
        private static readonly Action<ILogger, string, Exception> VccDeleteRequestSuccess;
        
        private static readonly Action<ILogger, string, decimal, Exception> VccModifyAmountRequestStarted;
        
        private static readonly Action<ILogger, string, string, Exception> VccModifyAmountRequestFailure;
        
        private static readonly Action<ILogger, string, decimal, Exception> VccModifyAmountRequestSuccess;
        
        private static readonly Action<ILogger, string, Exception> ResponseDeserializationFailed;
        
        private static readonly Action<ILogger, string, Exception> VccEditRequestStarted;
        
        private static readonly Action<ILogger, string, string, Exception> VccEditFailure;
        
        private static readonly Action<ILogger, string, Exception> VccEditSuccess;
    }
}