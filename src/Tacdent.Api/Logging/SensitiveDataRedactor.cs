using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using Tacdent.Api.Options;

namespace Tacdent.Api.Logging;

public sealed class SensitiveDataRedactor(IOptions<RequestLoggingOptions> options)
{
    private const string Mask = "***";

    public string RedactJson(string? body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return string.Empty;
        }

        try
        {
            var node = JsonNode.Parse(body);
            if (node is null)
            {
                return "[redacted]";
            }

            var sensitive = new HashSet<string>(
                options.Value.SensitiveJsonFields ?? [],
                StringComparer.OrdinalIgnoreCase);

            RedactNode(node, sensitive);
            return node.ToJsonString(new JsonSerializerOptions { WriteIndented = false });
        }
        catch (JsonException)
        {
            return "[redacted]";
        }
    }

    private static void RedactNode(JsonNode node, HashSet<string> sensitive)
    {
        switch (node)
        {
            case JsonObject obj:
                foreach (var property in obj.ToList())
                {
                    if (sensitive.Contains(property.Key))
                    {
                        obj[property.Key] = Mask;
                        continue;
                    }

                    if (property.Value is not null)
                    {
                        RedactNode(property.Value, sensitive);
                    }
                }

                break;

            case JsonArray array:
                foreach (var item in array)
                {
                    if (item is not null)
                    {
                        RedactNode(item, sensitive);
                    }
                }

                break;
        }
    }
}
