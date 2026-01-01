using Microsoft.Extensions.Options;
using Resend;

namespace Lefty.Email.Senders;

/// <summary />
public class ResendClientOptionsValidation : IValidateOptions<ResendClientOptions>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate( string? name, ResendClientOptions options )
    {
        if ( string.IsNullOrEmpty( options.ApiToken ) == true )
            return ValidateOptionsResult.Fail( "env:RESEND_APITOKEN is required" );

        return ValidateOptionsResult.Success;
    }
}