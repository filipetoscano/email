using Microsoft.Extensions.Options;

namespace Lefty.Email.Senders;

/// <summary />
public class AcsSenderOptionsValidation : IValidateOptions<AcsSenderOptions>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate( string? name, AcsSenderOptions options )
    {
        if ( string.IsNullOrEmpty( options.ConnectionString ) == true )
            return ValidateOptionsResult.Fail( "env:ACS_CONNECTIONSTRING is required" );

        return ValidateOptionsResult.Success;
    }
}