using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Text;

namespace Lefty.Email.Senders;

/// <summary />
public class MailkitSender : ISender
{
    private readonly MailkitSenderOptions _options;


    /// <summary />
    public MailkitSender( IOptions<MailkitSenderOptions> options )
    {
        _options = options.Value;
    }


    /// <inheritdoc />
    public async Task<string> SendAsync( Email message )
    {
        /*
         * Map
         */
        var m = new MimeMessage();

        m.From.Add( new MailboxAddress( message.From!.DisplayName, message.From.Email ) );

        foreach ( var to in message.To! )
            m.To.Add( new MailboxAddress( to.DisplayName, to.Email ) );

        m.Subject = message.Subject;


        /*
         * 
         */
        var builder = new BodyBuilder();

        builder.TextBody = message.TextBody;
        builder.HtmlBody = message.HtmlBody;

        if ( message.Attachments != null )
        {
            foreach ( var ea in message.Attachments )
            {
                byte[] bytes = ea.BinaryContent ?? [];

                if ( ea.ContentId != null )
                {
                    var lr = builder.LinkedResources.Add( ea.Filename, bytes );
                    lr.ContentId = ea.ContentId;
                }
                else
                {
                    builder.Attachments.Add( ea.Filename, bytes );
                }
            }
        }

        m.Body = builder.ToMessageBody();


        /*
         * Send
         */
        string resp = "";

        using ( var client = new SmtpClient() )
        {
            client.CheckCertificateRevocation = _options.CheckCertificateRevocation;

            client.MessageSent += ( _, ea ) =>
            {
                resp = ea.Response;
            };

            await client.ConnectAsync( _options.Host, _options.Port, _options.UseSsl );

            if ( _options.Username != null )
                await client.AuthenticateAsync( _options.Username, _options.Password );

            await client.SendAsync( m );
            await client.DisconnectAsync( true );
        }

        return resp;
    }
}