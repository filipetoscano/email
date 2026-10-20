namespace Lefty.Email;

/// <summary>
/// Expected failure while sending an email message, reported to
/// the user without a stack trace.
/// </summary>
public class SenderException : Exception
{
    /// <summary />
    public SenderException( string message )
        : base( message )
    {
    }

    /// <summary />
    public SenderException( string message, Exception innerException )
        : base( message, innerException )
    {
    }
}