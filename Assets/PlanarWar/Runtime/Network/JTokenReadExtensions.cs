using Newtonsoft.Json;
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

            if (typeof(T) == typeof(string))
            {
                object result = token.Type switch
                {
                    JTokenType.Object => token.ToString(Formatting.None),
                    JTokenType.Array => token.ToString(Formatting.None),
                    _ => token.ToObject<string>()
                };
                return (T)result;
            }

            return token.ToObject<T>();
        }
    }
}
