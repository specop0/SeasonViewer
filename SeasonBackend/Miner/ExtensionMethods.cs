using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace SeasonBackend.Miner
{
    public static class ExtensionMethods
    {
        public static T? Deserialize<T>(this string input)
        {
            var options = new JsonSerializerOptions()
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            };
            return JsonSerializer.Deserialize<T>(input, options);
        }

        public static string Serialize<T>(this T input)
        {
            return JsonSerializer.Serialize(input);
        }

        public static StringContent ToHttpContent<T>(this T input)
        {
            return new StringContent(input.Serialize(), Encoding.UTF8, "application/json");
        }
    }
}
