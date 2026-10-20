using Azure;
using Azure.Communication.Email;
using Azure.Identity;
using Microsoft.Extensions.Options;
using AcsEmailAddress = Azure.Communication.Email.EmailAddress;
using AcsEmailAttachment = Azure.Communication.Email.EmailAttachment;

namespace Lefty.Email.Senders;

/// <summary />
public class AcsSender : ISender
{
    private readonly AcsSenderOptions _options;


    /// <summary />
    public AcsSender( IOptions<AcsSenderOptions> options )
    {
        _options = options.Value;
    }


    /// <inheritdoc />
    public async Task<string> SendAsync( Email message )
    {
        /*
         * Map
         */
        var content = new EmailContent( message.Subject! )
        {
            PlainText = message.TextBody,
            Html = message.HtmlBody,
        };

        var recipients = new EmailRecipients(
            Map( message.To ),
            Map( message.Cc ),
            Map( message.Bcc ) );

        // ACS does not support display name on sender: this is configured
        // on the MailFrom address of the verified domain
        var m = new EmailMessage( message.From!.Email, recipients, content );

        if ( message.Attachments != null )
        {
            foreach ( var ea in message.Attachments )
            {
                var att = new AcsEmailAttachment(
                    ea.Name!,
                    ea.ContentType ?? "application/octet-stream",
                    BinaryData.FromBytes( ea.BinaryContent ?? [] ) );

                att.ContentId = ea.ContentId;

                m.Attachments.Add( att );
            }
        }


        /*
         * Send
         */
        EmailClient client;

        if ( _options.Endpoint != null )
            client = new EmailClient( new Uri( _options.Endpoint ), new DefaultAzureCredential() );
        else
            client = new EmailClient( _options.ConnectionString );

        var op = await client.SendAsync( WaitUntil.Completed, m );

        return $"{op.Id} {op.Value.Status}";
    }


    /// <summary />
    private static List<AcsEmailAddress> Map( Resend.EmailAddressList? list )
    {
        if ( list == null )
            return [];

        return list.Select( x => new AcsEmailAddress( x.Email, x.DisplayName ) ).ToList();
    }
}