using shortid;
using shortid.Configuration;

namespace HappyTravel.Gifu.Api.Infrastructure.Utils;

public static class UniqueIdGenerator
{
    public static string Get()
    {
        return ShortId.Generate(new GenerationOptions(useNumbers: true,
            useSpecialCharacters: false,
            length: 15
        ));
    }
}