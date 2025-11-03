using System.Text.Json;

namespace N2.McpCore;
public static class DictionaryExtensions
{
    public static string? GetStringArgument(this Dictionary<string, object?> arguments, string key)
    {
        if (arguments == null)
        {
            return null;
        }

        if (!arguments.TryGetValue(key, out object? value) || value == null)
        {
            return null;
        }

        if (value is JsonElement element)
        {
            return element.GetString();
        }

        return value.ToString();
    }

    public static T? GetArgument<T>(this Dictionary<string, object?> arguments, string key) where T : class
    {
        if (arguments == null)
        {
            return default;
        }

        if (!arguments.TryGetValue(key, out object? value) || value == null)
        {
            return null;
        }

        if (value is JsonElement element)
        {
            string? json = element.GetString();
            if (string.IsNullOrEmpty(json))
            {
                return default;
            }

            T? value1 = JsonSerializer.Deserialize<T>(json!);
            return value1;
        }

        if (value is T value2)
        {
            return (T)value2;
        }
        return default;
    }

    public static bool GetBoolArgument(this Dictionary<string, object?> arguments, string key)
    {
        if (arguments == null)
        {
            return false;
        }

        if (!arguments.TryGetValue(key, out object? value) || value == null)
        {
            return false;
        }

        if (value is JsonElement element)
        {
            return element.GetBoolean();
        }

        if (value is bool boolValue)
        {
            return boolValue;
        }

        return bool.TryParse(value.ToString(), out bool result) && result;
    }

    public static int? GetIntArgument(this Dictionary<string, object?> arguments, string key)
    {
        if (arguments == null)
        {
            return null;
        }

        if (!arguments.TryGetValue(key, out object? value) || value == null)
        {
            return null;
        }

        if (value is JsonElement element)
        {
            return element.GetInt32();
        }

        if (value is int intValue)
        {
            return intValue;
        }

        return int.TryParse(value.ToString(), out int result) ? result : null;
    }
}
