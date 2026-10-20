namespace Lefty.Email;

/// <summary />
public enum Sender
{
    /// <summary>
    /// SMTP, using Mailkit library.
    /// </summary>
    Smtp,

    /// <summary>
    /// Resend API.
    /// </summary>
    Resend,

    /// <summary>
    /// Azure Communication Services API.
    /// </summary>
    Acs,

    /// <summary>
    /// Null/always ok.
    /// </summary>
    Null,
}