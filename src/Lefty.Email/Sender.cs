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
}