using Azure.Communication.Email;
using Microsoft.Extensions.Options;

namespace Lefty.Email.Senders;

/// <summary />
public class AcsSenderOptionsValidation : IValidateOptions<AcsSenderOptions>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate( string? name, AcsSenderOptions options )
    {
        var hasConnectionString = string.IsNullOrEmpty( options.ConnectionString ) == false;
        var hasEndpoint = string.IsNullOrEmpty( options.Endpoint ) == false;

        if ( hasConnectionString == false && hasEndpoint == false )
            return ValidateOptionsResult.Fail( "env:ACS_CONNECTIONSTRING or env:ACS_ENDPOINT is required" );

        if ( hasConnectionString == true && hasEndpoint == true )
            return ValidateOptionsResult.Fail( "env:ACS_CONNECTIONSTRING and env:ACS_ENDPOINT are mutually exclusive" );

        if ( hasEndpoint == true )
        {
            // Bearer token authentication requires TLS
            if ( Uri.TryCreate( options.Endpoint, UriKind.Absolute, out var uri ) == false || uri.Scheme != Uri.UriSchemeHttps )
                return ValidateOptionsResult.Fail( "env:ACS_ENDPOINT must be an absolute https URL" );
        }

        if ( hasConnectionString == true )
        {
            // Client constructor parses the connection string, without network access
            try
            {
                _ = new EmailClient( options.ConnectionString );
            }
            catch
            {
                return ValidateOptionsResult.Fail( "env:ACS_CONNECTIONSTRING is not a valid connection string" );
            }
        }

        return ValidateOptionsResult.Success;
    }
}