using System;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Gifu.Api.Infrastructure.Logging;

public static partial class LoggerExtensions
{
    [LoggerMessage(81000, LogLevel.Warning, "Could not get client id for authenticated user")]
    static partial void ClientIdRetrievalFailure(ILogger logger);
    
    [LoggerMessage(81010, LogLevel.Information, "Processing VCC issue request for '{ReferenceCode}'. Amount: {Amount} {Currency},")]
    static partial void VccIssueRequestStarted(ILogger logger, string ReferenceCode, decimal Amount, string Currency);
    
    [LoggerMessage(81015, LogLevel.Error, "Processing VCC issue request for '{ReferenceCode}' failed: '{Error}'")]
    static partial void VccIssueRequestFailure(ILogger logger, string ReferenceCode, string Error);
    
    [LoggerMessage(81016, LogLevel.Information, "Processing VCC issue request for '{ReferenceCode}' completed successfully. UniqueId: '{UniqueId}'")]
    static partial void VccIssueRequestSuccess(ILogger logger, string ReferenceCode, string UniqueId);
    
    [LoggerMessage(0, LogLevel.Information, "Deleting VCC for '{ReferenceCode}'")]
    static partial void VccDeleteRequestStarted(ILogger logger, string ReferenceCode);
    
    [LoggerMessage(81021, LogLevel.Error, "Deleting VCC for '{ReferenceCode}' failed. '{Error}'")]
    static partial void VccDeleteRequestFailure(ILogger logger, string ReferenceCode, string Error);
    
    [LoggerMessage(81022, LogLevel.Information, "Deleting VCC for '{ReferenceCode}' completed successfully")]
    static partial void VccDeleteRequestSuccess(ILogger logger, string ReferenceCode);
    
    [LoggerMessage(81030, LogLevel.Information, "Modifying VCC amount for '{ReferenceCode}'. New value: {amount}")]
    static partial void VccModifyAmountRequestStarted(ILogger logger, string ReferenceCode, decimal amount);
    
    [LoggerMessage(81031, LogLevel.Error, "Modifying VCC amount for '{ReferenceCode}' failed. '{Error}'")]
    static partial void VccModifyAmountRequestFailure(ILogger logger, string ReferenceCode, string Error);
    
    [LoggerMessage(81032, LogLevel.Information, "Modifying VCC amount for '{ReferenceCode}' completed successfully. New value: {amount}")]
    static partial void VccModifyAmountRequestSuccess(ILogger logger, string ReferenceCode, decimal amount);
    
    [LoggerMessage(81040, LogLevel.Error, "Response deserialization failed: {Response}")]
    static partial void ResponseDeserializationFailed(ILogger logger, System.Exception exception, string Response);
    
    [LoggerMessage(81050, LogLevel.Information, "Editing VCC amount for '{ReferenceCode}' started")]
    static partial void VccEditRequestStarted(ILogger logger, string ReferenceCode);
    
    [LoggerMessage(81051, LogLevel.Error, "Editing VCC for '{ReferenceCode}' failed. '{Error}'")]
    static partial void VccEditFailure(ILogger logger, string ReferenceCode, string Error);
    
    [LoggerMessage(81052, LogLevel.Information, "Editing VCC for '{ReferenceCode}' completed successfully")]
    static partial void VccEditSuccess(ILogger logger, string ReferenceCode);
    
    
    
    public static void LogClientIdRetrievalFailure(this ILogger logger)
        => ClientIdRetrievalFailure(logger);
    
    public static void LogVccIssueRequestStarted(this ILogger logger, string ReferenceCode, decimal Amount, string Currency)
        => VccIssueRequestStarted(logger, ReferenceCode, Amount, Currency);
    
    public static void LogVccIssueRequestFailure(this ILogger logger, string ReferenceCode, string Error)
        => VccIssueRequestFailure(logger, ReferenceCode, Error);
    
    public static void LogVccIssueRequestSuccess(this ILogger logger, string ReferenceCode, string UniqueId)
        => VccIssueRequestSuccess(logger, ReferenceCode, UniqueId);
    
    public static void LogVccDeleteRequestStarted(this ILogger logger, string ReferenceCode)
        => VccDeleteRequestStarted(logger, ReferenceCode);
    
    public static void LogVccDeleteRequestFailure(this ILogger logger, string ReferenceCode, string Error)
        => VccDeleteRequestFailure(logger, ReferenceCode, Error);
    
    public static void LogVccDeleteRequestSuccess(this ILogger logger, string ReferenceCode)
        => VccDeleteRequestSuccess(logger, ReferenceCode);
    
    public static void LogVccModifyAmountRequestStarted(this ILogger logger, string ReferenceCode, decimal amount)
        => VccModifyAmountRequestStarted(logger, ReferenceCode, amount);
    
    public static void LogVccModifyAmountRequestFailure(this ILogger logger, string ReferenceCode, string Error)
        => VccModifyAmountRequestFailure(logger, ReferenceCode, Error);
    
    public static void LogVccModifyAmountRequestSuccess(this ILogger logger, string ReferenceCode, decimal amount)
        => VccModifyAmountRequestSuccess(logger, ReferenceCode, amount);
    
    public static void LogResponseDeserializationFailed(this ILogger logger, System.Exception exception, string Response)
        => ResponseDeserializationFailed(logger, exception, Response);
    
    public static void LogVccEditRequestStarted(this ILogger logger, string ReferenceCode)
        => VccEditRequestStarted(logger, ReferenceCode);
    
    public static void LogVccEditFailure(this ILogger logger, string ReferenceCode, string Error)
        => VccEditFailure(logger, ReferenceCode, Error);
    
    public static void LogVccEditSuccess(this ILogger logger, string ReferenceCode)
        => VccEditSuccess(logger, ReferenceCode);
}