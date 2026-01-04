using Resend;
using System.Text.Json.Serialization;

namespace Lefty.Email;

/// <summary>
/// Email message.
/// </summary>
public class Email
{
    /// <summary>
    /// Sender.
    /// </summary>
    public EmailAddress? From { get; set; }

    /// <summary>
    /// Recipient.
    /// </summary>
    public EmailAddressList? To { get; set; }

    /// <summary>
    /// Carbon copy.
    /// </summary>
    [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
    public EmailAddressList? Cc { get; set; }

    /// <summary>
    /// Blind carbon copy.
    /// </summary>
    [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
    public EmailAddressList? Bcc { get; set; }

    /// <summary>
    /// Subject.
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// Plain text body.
    /// </summary>
    [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
    public string? TextBody { get; set; }

    /// <summary>
    /// HTML body.
    /// </summary>
    [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
    public string? HtmlBody { get; set; }


    /// <summary>
    /// List of attachments.
    /// </summary>
    [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
    public List<EmailAttachment>? Attachments { get; set; }
}


/// <summary />
public class EmailAttachment
{
    /// <summary>
    /// File path, relative to directory of JSON file or current
    /// directory if JSON is piped into tool.
    /// </summary>
    public required string Filename { get; set; }

    /// <summary>
    /// Content identifier, when referring to attachments from
    /// the HTML body.
    /// </summary>
    public string? ContentId { get; set; }

    /// <summary>
    /// MIME type of the attachment.
    /// </summary>
    public string? ContentType { get; set; }


    /// <summary>
    /// Name only of the file attachment.
    /// </summary>
    [JsonIgnore]
    public string? Name { get; set; }

    /// <summary>
    /// Binary content.
    /// </summary>
    [JsonIgnore]
    public byte[]? BinaryContent { get; set; }
}