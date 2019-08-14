using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Varwin.Data
{
    public static class JsonSerializer
    {
        public static string ToJson(this IJsonSerializable self)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto
            };
            var json = JsonConvert.SerializeObject(self, settings);
            return json;
        }

        public static TEntity JsonDeserialize<TEntity>(this string json) where TEntity : IJsonSerializable
        {
            try
            {
                JsonSerializerSettings settings = new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.Auto
                };
                return JsonConvert.DeserializeObject<TEntity>(json, settings);
            }
            catch (Exception ex)
            {
                throw new Exception($"Can't deserialize {typeof(TEntity)} json. Message: {ex.Message}");
            }

        }

        public static List<TEntity> JsonDeserializeList<TEntity>(this string json) where TEntity : IJsonSerializable
        {
            try
            {
                JsonSerializerSettings settings = new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.Auto
                };
                return JsonConvert.DeserializeObject<List<TEntity>>(json, settings);
            }
            catch
            {
                throw new Exception($"Can't deserialize {typeof(TEntity)} json");
            }

        }
    }

    public interface IJsonSerializable
    {

    }
}
