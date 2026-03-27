using McpCore;

namespace N2.McpCore.Unittests;

public class UsingSessionCodeGenerator
{
    private SessionCodeGenerator _generator = null!;

    [SetUp]
    public void Setup()
    {
        _generator = new SessionCodeGenerator();
    }

    [Test]
    public void GenerateCode_ReturnsExactlySixCharacters()
    {
        var code = _generator.GenerateCode();

        Assert.That(code.Length, Is.EqualTo(6));
    }

    [Test]
    public void GenerateCode_ReturnsOnlyUppercaseAlphanumericChars()
    {
        var code = _generator.GenerateCode();

        Assert.That(code, Does.Match("^[A-Z0-9]{6}$"));
    }

    [Test]
    public void GenerateCode_ProducesNonNullNonEmptyString()
    {
        var code = _generator.GenerateCode();

        Assert.That(code, Is.Not.Null);
        Assert.That(code, Is.Not.Empty);
    }

    [Test]
    public void GenerateCode_CalledRepeatedly_DoesNotAlwaysReturnSameValue()
    {
        // With 36^6 = ~2.18 billion possibilities, two calls should almost never match.
        var codes = Enumerable.Range(0, 20).Select(_ => _generator.GenerateCode()).ToList();
        var distinct = codes.Distinct().Count();

        Assert.That(distinct, Is.GreaterThan(1));
    }

    [Test]
    public void IsValidCode_ValidUppercaseCode_ReturnsTrue()
    {
        Assert.That(_generator.IsValidCode("ABC123"), Is.True);
    }

    [Test]
    public void IsValidCode_ValidLowercaseCode_ReturnsTrue()
    {
        // The method normalises to uppercase internally.
        Assert.That(_generator.IsValidCode("abc123"), Is.True);
    }

    [Test]
    public void IsValidCode_MixedCaseCode_ReturnsTrue()
    {
        Assert.That(_generator.IsValidCode("AbCdEf"), Is.True);
    }

    [Test]
    public void IsValidCode_AllDigits_ReturnsTrue()
    {
        Assert.That(_generator.IsValidCode("123456"), Is.True);
    }

    [Test]
    public void IsValidCode_AllLetters_ReturnsTrue()
    {
        Assert.That(_generator.IsValidCode("ABCDEF"), Is.True);
    }

    [Test]
    public void IsValidCode_NullInput_ReturnsFalse()
    {
        Assert.That(_generator.IsValidCode(null!), Is.False);
    }

    [Test]
    public void IsValidCode_EmptyString_ReturnsFalse()
    {
        Assert.That(_generator.IsValidCode(string.Empty), Is.False);
    }

    [Test]
    public void IsValidCode_TooShort_ReturnsFalse()
    {
        Assert.That(_generator.IsValidCode("AB12"), Is.False);
    }

    [Test]
    public void IsValidCode_TooLong_ReturnsFalse()
    {
        Assert.That(_generator.IsValidCode("ABC1234"), Is.False);
    }

    [Test]
    public void IsValidCode_ExactlyFiveChars_ReturnsFalse()
    {
        Assert.That(_generator.IsValidCode("ABCDE"), Is.False);
    }

    [Test]
    public void IsValidCode_ExactlySevenChars_ReturnsFalse()
    {
        Assert.That(_generator.IsValidCode("ABCDEFG"), Is.False);
    }

    [Test]
    public void IsValidCode_ContainsSpace_ReturnsFalse()
    {
        Assert.That(_generator.IsValidCode("ABC 12"), Is.False);
    }

    [Test]
    public void IsValidCode_ContainsSpecialChar_ReturnsFalse()
    {
        Assert.That(_generator.IsValidCode("ABC!23"), Is.False);
    }

    [Test]
    public void IsValidCode_ContainsHyphen_ReturnsFalse()
    {
        Assert.That(_generator.IsValidCode("ABC-23"), Is.False);
    }

    [Test]
    public void IsValidCode_GeneratedCode_AlwaysValid()
    {
        for (int i = 0; i < 50; i++)
        {
            var code = _generator.GenerateCode();
            Assert.That(_generator.IsValidCode(code), Is.True, $"Generated code '{code}' failed validation");
        }
    }
}
