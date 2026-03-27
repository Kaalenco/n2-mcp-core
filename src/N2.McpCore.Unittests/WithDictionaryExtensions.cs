using System.Text.Json;
using N2.McpCore;

namespace N2.McpCore.Unittests;

public class WithDictionaryExtensions
{
    // ── GetStringArgument ────────────────────────────────────────────────────

    [Test]
    public void GetStringArgument_NullDictionary_ReturnsNull()
    {
        Dictionary<string, object?> dict = null!;

        Assert.That(dict.GetStringArgument("key"), Is.Null);
    }

    [Test]
    public void GetStringArgument_MissingKey_ReturnsNull()
    {
        var dict = new Dictionary<string, object?>();

        Assert.That(dict.GetStringArgument("missing"), Is.Null);
    }

    [Test]
    public void GetStringArgument_NullValue_ReturnsNull()
    {
        var dict = new Dictionary<string, object?> { ["key"] = null };

        Assert.That(dict.GetStringArgument("key"), Is.Null);
    }

    [Test]
    public void GetStringArgument_ClrStringValue_ReturnsValue()
    {
        var dict = new Dictionary<string, object?> { ["key"] = "hello" };

        Assert.That(dict.GetStringArgument("key"), Is.EqualTo("hello"));
    }

    [Test]
    public void GetStringArgument_JsonElementString_ReturnsValue()
    {
        using var doc = JsonDocument.Parse("\"world\"");
        var dict = new Dictionary<string, object?> { ["key"] = doc.RootElement.Clone() };

        Assert.That(dict.GetStringArgument("key"), Is.EqualTo("world"));
    }

    [Test]
    public void GetStringArgument_EmptyString_ReturnsEmptyString()
    {
        var dict = new Dictionary<string, object?> { ["key"] = string.Empty };

        Assert.That(dict.GetStringArgument("key"), Is.EqualTo(string.Empty));
    }

    // ── GetBoolArgument ──────────────────────────────────────────────────────

    [Test]
    public void GetBoolArgument_NullDictionary_ReturnsFalse()
    {
        Dictionary<string, object?> dict = null!;

        Assert.That(dict.GetBoolArgument("key"), Is.False);
    }

    [Test]
    public void GetBoolArgument_MissingKey_ReturnsFalse()
    {
        var dict = new Dictionary<string, object?>();

        Assert.That(dict.GetBoolArgument("missing"), Is.False);
    }

    [Test]
    public void GetBoolArgument_NullValue_ReturnsFalse()
    {
        var dict = new Dictionary<string, object?> { ["key"] = null };

        Assert.That(dict.GetBoolArgument("key"), Is.False);
    }

    [Test]
    public void GetBoolArgument_ClrTrue_ReturnsTrue()
    {
        var dict = new Dictionary<string, object?> { ["key"] = true };

        Assert.That(dict.GetBoolArgument("key"), Is.True);
    }

    [Test]
    public void GetBoolArgument_ClrFalse_ReturnsFalse()
    {
        var dict = new Dictionary<string, object?> { ["key"] = false };

        Assert.That(dict.GetBoolArgument("key"), Is.False);
    }

    [Test]
    public void GetBoolArgument_JsonElementTrue_ReturnsTrue()
    {
        using var doc = JsonDocument.Parse("true");
        var dict = new Dictionary<string, object?> { ["key"] = doc.RootElement.Clone() };

        Assert.That(dict.GetBoolArgument("key"), Is.True);
    }

    [Test]
    public void GetBoolArgument_JsonElementFalse_ReturnsFalse()
    {
        using var doc = JsonDocument.Parse("false");
        var dict = new Dictionary<string, object?> { ["key"] = doc.RootElement.Clone() };

        Assert.That(dict.GetBoolArgument("key"), Is.False);
    }

    [Test]
    public void GetBoolArgument_StringTrue_ReturnsTrue()
    {
        var dict = new Dictionary<string, object?> { ["key"] = "true" };

        Assert.That(dict.GetBoolArgument("key"), Is.True);
    }

    [Test]
    public void GetBoolArgument_StringFalse_ReturnsFalse()
    {
        var dict = new Dictionary<string, object?> { ["key"] = "false" };

        Assert.That(dict.GetBoolArgument("key"), Is.False);
    }

    [Test]
    public void GetBoolArgument_InvalidString_ReturnsFalse()
    {
        var dict = new Dictionary<string, object?> { ["key"] = "yes" };

        Assert.That(dict.GetBoolArgument("key"), Is.False);
    }

    // ── GetIntArgument ───────────────────────────────────────────────────────

    [Test]
    public void GetIntArgument_NullDictionary_ReturnsNull()
    {
        Dictionary<string, object?> dict = null!;

        Assert.That(dict.GetIntArgument("key"), Is.Null);
    }

    [Test]
    public void GetIntArgument_MissingKey_ReturnsNull()
    {
        var dict = new Dictionary<string, object?>();

        Assert.That(dict.GetIntArgument("missing"), Is.Null);
    }

    [Test]
    public void GetIntArgument_NullValue_ReturnsNull()
    {
        var dict = new Dictionary<string, object?> { ["key"] = null };

        Assert.That(dict.GetIntArgument("key"), Is.Null);
    }

    [Test]
    public void GetIntArgument_ClrInt_ReturnsValue()
    {
        var dict = new Dictionary<string, object?> { ["key"] = 42 };

        Assert.That(dict.GetIntArgument("key"), Is.EqualTo(42));
    }

    [Test]
    public void GetIntArgument_JsonElementNumber_ReturnsValue()
    {
        using var doc = JsonDocument.Parse("99");
        var dict = new Dictionary<string, object?> { ["key"] = doc.RootElement.Clone() };

        Assert.That(dict.GetIntArgument("key"), Is.EqualTo(99));
    }

    [Test]
    public void GetIntArgument_StringNumber_ReturnsValue()
    {
        var dict = new Dictionary<string, object?> { ["key"] = "7" };

        Assert.That(dict.GetIntArgument("key"), Is.EqualTo(7));
    }

    [Test]
    public void GetIntArgument_StringNonNumber_ReturnsNull()
    {
        var dict = new Dictionary<string, object?> { ["key"] = "notanumber" };

        Assert.That(dict.GetIntArgument("key"), Is.Null);
    }

    [Test]
    public void GetIntArgument_NegativeValue_ReturnsNegative()
    {
        var dict = new Dictionary<string, object?> { ["key"] = -5 };

        Assert.That(dict.GetIntArgument("key"), Is.EqualTo(-5));
    }

    [Test]
    public void GetIntArgument_Zero_ReturnsZero()
    {
        var dict = new Dictionary<string, object?> { ["key"] = 0 };

        Assert.That(dict.GetIntArgument("key"), Is.EqualTo(0));
    }

    // ── GetArgument<T> ───────────────────────────────────────────────────────

    [Test]
    public void GetArgumentT_NullDictionary_ReturnsNull()
    {
        Dictionary<string, object?> dict = null!;

        Assert.That(dict.GetArgument<string>("key"), Is.Null);
    }

    [Test]
    public void GetArgumentT_MissingKey_ReturnsNull()
    {
        var dict = new Dictionary<string, object?>();

        Assert.That(dict.GetArgument<string>("missing"), Is.Null);
    }

    [Test]
    public void GetArgumentT_ClrTypeMatch_ReturnsValue()
    {
        var dict = new Dictionary<string, object?> { ["key"] = "direct" };

        Assert.That(dict.GetArgument<string>("key"), Is.EqualTo("direct"));
    }

    [Test]
    public void GetArgumentT_JsonElementWithSerializedJson_DeserializesValue()
    {
        // GetArgument<T> calls element.GetString() then deserialises the resulting JSON string.
        using var doc = JsonDocument.Parse("\"\\\"hello\\\"\"");   // JSON string whose value is "hello"
        // Simpler: pass a JSON string that itself is valid JSON for the target type.
        using var outer = JsonDocument.Parse("\"42\"");            // string value "42" is valid JSON for int? No.
        // GetArgument<T> treats the element as a string and then deserialises that string as T.
        // For a List<int>, the element string value must be "[1,2,3]".
        using var stringElement = JsonDocument.Parse("\"[1,2,3]\"");
        var dict = new Dictionary<string, object?> { ["key"] = stringElement.RootElement.Clone() };

        var result = dict.GetArgument<List<int>>("key");

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.EqualTo(new List<int> { 1, 2, 3 }));
    }

    [Test]
    public void GetArgumentT_JsonElementWithEmptyString_ReturnsDefault()
    {
        // element.GetString() returns "" → treated as empty JSON → returns default
        using var doc = JsonDocument.Parse("\"\"");
        var dict = new Dictionary<string, object?> { ["key"] = doc.RootElement.Clone() };

        Assert.That(dict.GetArgument<string>("key"), Is.Null);
    }
}
