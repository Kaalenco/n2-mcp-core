using McpCore;

namespace N2.McpCore.Unittests;

public class UsingResponse
{
    [Test]
    public void Ok_ReturnsSuccessTrue()
    {
        var result = Response.Ok();

        Assert.That(result.Success, Is.True);
        Assert.That(result.Message, Is.Null);
    }

    [Test]
    public void Ok_ReturnsSameInstanceEachTime()
    {
        var first = Response.Ok();
        var second = Response.Ok();

        Assert.That(first, Is.SameAs(second));
    }

    [Test]
    public void Ok_WithMessage_ReturnsSuccessWithMessage()
    {
        var result = Response.Ok("operation completed");

        Assert.That(result.Success, Is.True);
        Assert.That(result.Message, Is.EqualTo("operation completed"));
    }

    [Test]
    public void Ok_WithMessage_ReturnsDifferentInstanceEachTime()
    {
        var first = Response.Ok("msg");
        var second = Response.Ok("msg");

        Assert.That(first, Is.Not.SameAs(second));
    }

    [Test]
    public void Fail_ReturnsSuccessFalse()
    {
        var result = Response.Fail();

        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.Null);
    }

    [Test]
    public void Fail_ReturnsSameInstanceEachTime()
    {
        var first = Response.Fail();
        var second = Response.Fail();

        Assert.That(first, Is.SameAs(second));
    }

    [Test]
    public void Fail_WithMessage_ReturnsFailureWithMessage()
    {
        var result = Response.Fail("something broke");

        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo("something broke"));
    }

    [Test]
    public void Ok_WithValue_ReturnsTypedResponseWithValue()
    {
        var result = Response.Ok(42);

        Assert.That(result, Is.InstanceOf<Response<int>>());
        Assert.That(result.Success, Is.True);
        var typed = (Response<int>)result;
        Assert.That(typed.Value, Is.EqualTo(42));
    }

    [Test]
    public void Ok_WithMessageAndValue_ReturnsTypedResponseWithBoth()
    {
        var result = Response.Ok("found it", "hello");

        Assert.That(result, Is.InstanceOf<Response<string>>());
        Assert.That(result.Success, Is.True);
        Assert.That(result.Message, Is.EqualTo("found it"));
        var typed = (Response<string>)result;
        Assert.That(typed.Value, Is.EqualTo("hello"));
    }

    [Test]
    public void ResponseT_DefaultConstructor_HasNullValue()
    {
        var result = new Response<string>();

        Assert.That(result.Value, Is.Null);
    }

    [Test]
    public void ResponseT_ValueConstructor_SetsSuccessTrue()
    {
        var result = new Response<int>(99);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Value, Is.EqualTo(99));
        Assert.That(result.Message, Is.Null);
    }

    [Test]
    public void ResponseT_FullConstructor_SetsAllProperties()
    {
        var result = new Response<bool>(false, "not found", false);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo("not found"));
        Assert.That(result.Value, Is.False);
    }

    [Test]
    public void ResponseT_InheritsFromResponse()
    {
        Response result = Response.Ok(123);

        Assert.That(result, Is.InstanceOf<Response>());
    }

    [Test]
    public void Ok_WithEmptyMessage_StoresEmptyString()
    {
        var result = Response.Ok(string.Empty);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Message, Is.EqualTo(string.Empty));
    }

    [Test]
    public void Fail_WithEmptyMessage_StoresEmptyString()
    {
        var result = Response.Fail(string.Empty);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo(string.Empty));
    }
}
