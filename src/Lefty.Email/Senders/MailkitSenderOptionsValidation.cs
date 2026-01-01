using Microsoft.Extensions.Options;

namespace Lefty.Email.Senders;

/// <summary />
public class MailkitSenderOptionsValidation : IValidateOptions<MailkitSenderOptions>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate( string? name, MailkitSenderOptions options )
    {
        if ( options.Host == null )
            return ValidateOptionsResult.Fail( "env:SMTP_HOST is required" );

        if ( options.Username != null && options.Password == null )
            return ValidateOptionsResult.Fail( "env:SMTP_PASSWORD is required when username is specified" );

        return ValidateOptionsResult.Success;
    }
}