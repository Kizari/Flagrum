using System;
using System.Text;
using Newtonsoft.Json;

namespace Flagrum.Core.Extensions
{
    public static class Base64Extensions
    {
        public static string ToBase64String<TModel>(this TModel model)
        {
            var json = JsonConvert.SerializeObject(model);
            var bytes = Encoding.ASCII.GetBytes(json);
            return Convert.ToBase64String(bytes);
        }

        public static TModel FromBase64String<TModel>(this string base64)
        {
            var bytes = Convert.FromBase64String(base64);
            var json = Encoding.ASCII.GetString(bytes);
            return JsonConvert.DeserializeObject<TModel>(json);
        }
    }
}