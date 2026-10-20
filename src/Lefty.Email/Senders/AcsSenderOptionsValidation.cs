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

        if ( hasEndpoint == true && Uri.TryCreate( options.Endpoint, UriKind.Absolute, out _ ) == false )
            return ValidateOptionsResult.Fail( "env:ACS_ENDPOINT must be an absolute URL" );

        return ValidateOptionsResult.Success;
    }
}