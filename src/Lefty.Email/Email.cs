using Resend;
using System.Text.Json.Serialization;

namespace Lefty.Email;

/// <summary />
public class Email
{
    /// <summary />
    public EmailAddress? From { get; set; }

    /// <summary />
    public EmailAddressList? To { get; set; }

    /// <summary />
    [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
    public EmailAddressList? Cc { get; set; }

    /// <summary />
    [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
    public EmailAddressList? Bcc { get; set; }

    /// <summary />
    public string? Subject { get; set; }

    /// <summary />
    public string? TextBody { get; set; }

    /// <summary />
    public string? HtmlBody { get; set; }


    /// <summary />
    public List<EmailAttachment>? Attachments { get; set; }
}


/// <summary />
public class EmailAttachment
{
    /// <summary />
    public required string Filename { get; set; }

    /// <summary />
    public string? ContentId { get; set; }

    /// <summary />
    public string? ContentType { get; set; }


    /// <summary />
    [JsonIgnore]
    public string? Name { get; set; }

    /// <summary />
    [JsonIgnore]
    public string? TextContent { get; set; }

    /// <summary />
    [JsonIgnore]
    public byte[]? BinaryContent { get; set; }
}