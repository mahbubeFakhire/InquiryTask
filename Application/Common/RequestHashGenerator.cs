using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Application.Common;


public static class RequestHashGenerator
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public static string Generate<T>(T request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var node = JsonSerializer.SerializeToNode(request, SerializerOptions)
            ?? throw new JsonException(
                "The request must serialize to a non-null JSON value.");

        var canonicalNode = Canonicalize(node)!;
        var canonicalJson = canonicalNode.ToJsonString(SerializerOptions);

        var bytes = Encoding.UTF8.GetBytes(canonicalJson);
        var hash = SHA256.HashData(bytes);

        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static JsonNode? Canonicalize(JsonNode? node)
    {
        if (node is JsonObject jsonObject)
        {
            var result = new JsonObject();

            foreach (var property in jsonObject.OrderBy(
                x => x.Key,
                StringComparer.Ordinal))
            {
                result[property.Key] = Canonicalize(property.Value);
            }

            return result;
        }

        if (node is JsonArray jsonArray)
        {
            var result = new JsonArray();

            // ترتیب عناصر آرایه حفظ می‌شود.
            foreach (var item in jsonArray)
            {
                result.Add(Canonicalize(item));
            }

            return result;
        }

        return node?.DeepClone();
    }
}
