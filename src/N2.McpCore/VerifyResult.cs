namespace McpCore;

/// <summary>
/// Accumulates validation failures and remarks during a verification pass.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Success"/> starts as <see langword="true"/> and <see cref="Completed"/> starts
/// as <see langword="false"/>. A verification must be explicitly finalized by calling either
/// <see cref="Complete"/> (all conditions passed) or <see cref="CompleteWithFailure"/>
/// (one or more conditions failed). Any further calls after finalization throw
/// <see cref="NotSupportedException"/>.
/// </para>
/// <para>
/// Typical usage pattern:
/// <list type="number">
///   <item>Create with <see cref="Start"/> to record what is being verified.</item>
///   <item>Call <see cref="Fail"/> for each condition that does not hold (accumulates; does not finalize).</item>
///   <item>Call <see cref="Remark"/> to add context without affecting the outcome.</item>
///   <item>Finalize: call <see cref="Complete"/> if <see cref="Success"/> is still
///         <see langword="true"/>, otherwise call <see cref="CompleteWithFailure"/>.</item>
///   <item>Call <see cref="ThrowIfFailed"/> to surface accumulated errors as an
///         <see cref="ArgumentException"/>. Also throws if the result was never completed.</item>
/// </list>
/// </para>
/// </remarks>
public class VerifyResult
{
    private readonly List<string> messages = [];

    public VerifyResult()
    {
        Success = true;
        Completed = false;
    }

    /// <summary>
    /// Records a failure message and marks this result as unsuccessful.
    /// </summary>
    public void Fail(string message)
    {
        VerifyCompleted();
        messages.Add(message);
        Success = false;
    }

    /// <summary>
    /// Completes the verification with an optional failure message. 
    /// If a message is provided, the message is added to the verification messages.
    /// </summary>
    /// <param name="message"></param>
    public void CompleteWithFailure(string? message)
    {
        VerifyCompleted();
        if (!string.IsNullOrEmpty(message))
        {
            messages.Add(message!);
        }
        Success = false;
        Completed = true;
    }

    public void Complete(string? message)
    {
        VerifyCompleted();
        if (!string.IsNullOrEmpty(message))
        {
            messages.Add(message!);
        }
        Success = true;
        Completed = true;
    }

    /// <summary>
    /// Adds an informational message without changing <see cref="Success"/>.
    /// </summary>
    public void Remark(string message)
    {
        VerifyCompleted();
        messages.Add(message);
    }

    private void VerifyCompleted()
    {
        if (Completed)
        {
            throw new NotSupportedException("Verification already completed");
        }
    }

    /// <summary>
    /// Gets whether the verification passed.
    /// </summary>
    public bool Success { get; private set; }
    public bool Failure => !Success;

    public bool Completed { get; private set; }

    public IEnumerable<string> Messages => messages;

    public static VerifyResult Start(string message)
    {
        var result = new VerifyResult();
        result.Remark(message);
        return result;
    }

    public void ThrowIfFailed()
    {
        if (!Completed || Failure)
        {
            throw new ArgumentException(string.Join(", ", messages));
        }
    }
}