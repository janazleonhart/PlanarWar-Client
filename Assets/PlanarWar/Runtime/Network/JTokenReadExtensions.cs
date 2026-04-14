using Newtonsoft.Json.Linq;

namespace PlanarWar.Client.Network
{
    public static class JTokenReadExtensions
    {
        public static T Read<T>(this JToken token)
        {
            if (token == null || token.Type == JTokenType.Null || token.Type == JTokenType.Undefined)
            {
                return default;
            }

            return token.ToObject<T>();
        }
    }
}
