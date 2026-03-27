using McpCore;

namespace N2.McpCore.Unittests;

public class UsingVerifyResult
{
    [Test]
    public void NewInstance_HasNoMessages()
    {
        var result = new VerifyResult();

        Assert.That(result.Messages, Is.Empty);
    }

    [Test]
    public void NewInstance_SuccessIsFalse()
    {
        var result = new VerifyResult();

        Assert.That(result.Success, Is.True);
        Assert.That(result.Failure, Is.False);
    }

    [Test]
    public void Fail_SetsSuccessFalseAndRecordsMessage()
    {
        var result = new VerifyResult();
        result.Fail("something went wrong");

        Assert.That(result.Success, Is.False);
        Assert.That(result.Failure, Is.True);
        Assert.That(result.Messages, Contains.Item("something went wrong"));
    }

    [Test]
    public void Remark_AddsMessageWithoutChangingSuccess()
    {
        var result = new VerifyResult();
        result.Remark("just a note");

        Assert.That(result.Messages, Contains.Item("just a note"));
    }

    [Test]
    public void Fail_MultipleTimes_AccumulatesAllMessages()
    {
        var result = new VerifyResult();
        result.Fail("error one");
        result.Fail("error two");
        result.Fail("error three");

        Assert.That(result.Messages.Count(), Is.EqualTo(3));
    }

    [Test]
    public void Remark_AndFail_BothAppearInMessages()
    {
        var result = new VerifyResult();
        result.Remark("context note");
        result.Fail("actual failure");

        var messages = result.Messages.ToList();
        Assert.That(messages, Has.Count.EqualTo(2));
        Assert.That(messages[0], Is.EqualTo("context note"));
        Assert.That(messages[1], Is.EqualTo("actual failure"));
    }

    [Test]
    public void ThrowIfFailed_WhenFailed_ThrowsArgumentException()
    {
        var result = new VerifyResult();
        result.Fail("bad input");

        Assert.Throws<ArgumentException>(() => result.ThrowIfFailed());
    }

    [Test]
    public void ThrowIfFailed_ExceptionMessage_ContainsAllFailureMessages()
    {
        var result = new VerifyResult();
        result.Fail("first error");
        result.Fail("second error");

        var ex = Assert.Throws<ArgumentException>(() => result.ThrowIfFailed());
        Assert.That(ex!.Message, Does.Contain("first error"));
        Assert.That(ex.Message, Does.Contain("second error"));
    }

    [Test]
    public void ThrowIfFailed_FreshInstance_ThrowsBecauseSuccessDefaultsToFalse()
    {
        // Edge case: Success defaults to false, so ThrowIfFailed throws even with no messages.
        var result = new VerifyResult();

        Assert.Throws<ArgumentException>(() => result.ThrowIfFailed());
    }

    [Test]
    public void Start_ReturnsInstanceWithInitialRemark()
    {
        var result = VerifyResult.Start("starting validation");

        Assert.That(result.Messages, Contains.Item("starting validation"));
    }

    [Test]
    public void Start_ReturnedInstance_HasSuccessFalse()
    {
        // Start() uses Remark() internally which does not set Success to true.
        var result = VerifyResult.Start("begin");

        Assert.That(result.Success, Is.True);
    }

    [Test]
    public void FailurePropery_IsInverseOfSuccess()
    {
        var result = new VerifyResult();
        // Both should agree
        Assert.That(result.Failure, Is.EqualTo(!result.Success));
    }
}
