using Azure;
using Azure.Communication.Email;
using Azure.Identity;
using Microsoft.Extensions.Options;
using MimeKit;
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
                // ACS only accepts an allow-list of content types, so infer
                // from the file extension rather than using a generic type
                var contentType = string.IsNullOrEmpty( ea.ContentType ) == false
                    ? ea.ContentType
                    : MimeTypes.GetMimeType( ea.Name! );

                var att = new AcsEmailAttachment(
                    ea.Name!,
                    contentType,
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

        EmailSendOperation op;

        try
        {
            op = await client.SendAsync( WaitUntil.Completed, m );
        }
        catch ( AuthenticationFailedException ex )
        {
            throw new SenderException( $"unable to authenticate with Entra ID: {ex.Message}", ex );
        }
        catch ( RequestFailedException ex )
        {
            throw new SenderException( $"ACS request failed: {FirstLine( ex.Message )}", ex );
        }
        catch ( AggregateException ex )
            when ( ex.InnerExceptions.All( x => x is RequestFailedException ) == true )
        {
            // Raised once the retry policy has been exhausted
            throw new SenderException( $"ACS request failed: {FirstLine( ex.InnerExceptions[ ^1 ].Message )}", ex );
        }

        if ( op.Value.Status != EmailSendStatus.Succeeded )
            throw new SenderException( $"ACS send operation {op.Id} ended with status {op.Value.Status}" );

        return $"{op.Id} {op.Value.Status}";
    }


    /// <summary />
    private static List<AcsEmailAddress> Map( Resend.EmailAddressList? list )
    {
        if ( list == null )
            return [];

        return list.Select( x => new AcsEmailAddress( x.Email, x.DisplayName ) ).ToList();
    }


    /// <summary />
    private static string FirstLine( string value )
    {
        var ix = value.IndexOfAny( [ '\r', '\n' ] );

        return ix < 0 ? value : value[ ..ix ];
    }
}