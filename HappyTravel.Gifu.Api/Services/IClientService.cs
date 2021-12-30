using CSharpFunctionalExtensions;

namespace HappyTravel.Gifu.Api.Services;

public interface IClientService
{
    Result<string> GetId();
}