namespace Lefty.Email;

/// <summary>
/// Email sender.
/// </summary>
public interface ISender
{
    /// <summary>
    /// Sends an email message.
    /// </summary>
    Task SendAsync( Email message );
}