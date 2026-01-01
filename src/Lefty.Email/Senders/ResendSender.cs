using Resend;

namespace Lefty.Email.Senders;

/// <summary />
public class ResendSender : ISender
{
    private readonly IResend _resend;

    /// <summary />
    public ResendSender( IResend resend )
    {
        _resend = resend;
    }


    /// <inheritdoc />
    public async Task Send( Email message )
    {
        var m = new EmailMessage();

        m.From = message.From!;
        m.To = message.To!;
        m.Cc = message.Cc;
        m.Bcc = message.Bcc;

        m.Subject = message.Subject!;

        m.TextBody = message.TextBody;
        m.HtmlBody = message.HtmlBody;

        if ( message.Attachments != null )
        {
            m.Attachments = message.Attachments.Select( x =>
            {
                if ( x.TextContent != null )
                {
                    return new Resend.EmailAttachment()
                    {
                        Filename = x.Name!,
                        ContentId = x.ContentId,
                        ContentType = x.ContentType,
                        Content = x.TextContent,
                    };
                }
                else
                {
                    return new Resend.EmailAttachment()
                    {
                        Filename = x.Name!,
                        ContentId = x.ContentId,
                        ContentType = x.ContentType,
                        Content = x.BinaryContent ?? [],
                    };
                }
            } ).ToList();
        }

        await _resend.EmailSendAsync( m );
    }
}