using Resend;
using System.Text;

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
    public async Task SendAsync( Email message )
    {
        /*
         * Map
         */
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
                if ( IsTextContent( x ) == true )
                {
                    var text = Encoding.UTF8.GetString( x.BinaryContent ?? [] );

                    return new Resend.EmailAttachment()
                    {
                        Filename = x.Name!,
                        ContentId = x.ContentId,
                        ContentType = x.ContentType,
                        Content = text,
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


        /*
         * Send
         */
        var resp = await _resend.EmailSendAsync( m );

        Console.WriteLine( "{0}", resp.Content );
    }


    /// <summary />
    private bool IsTextContent( EmailAttachment attach )
    {
        /*
         * 
         */
        if ( attach.ContentType != null )
        {
            if ( attach.ContentType.StartsWith( "text/" ) == true )
                return true;

            if ( attach.ContentType.StartsWith( "application/json" ) == true )
                return true;

            if ( attach.ContentType.StartsWith( "application/xml" ) == true )
                return true;

            if ( attach.ContentType.EndsWith( "+xml" ) == true )
                return true;
        }


        /*
         * 
         */
        var path = Path.GetExtension( attach.Filename )?.ToLowerInvariant();

        if ( path == ".txt" )
            return true;

        if ( path == ".html" )
            return true;

        if ( path == ".json" )
            return true;

        if ( path == ".xml" )
            return true;

        if ( path == ".md" )
            return true;

        if ( path == ".ics" )
            return true;

        return false;
    }
}