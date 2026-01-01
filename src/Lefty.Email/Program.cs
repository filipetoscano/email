using Lefty.Email.Senders;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Resend;
using System.Reflection;
using System.Text.Json;

namespace Lefty.Email;

/// <summary />
[Command( "lemail", Description = "Email sender" )]
[VersionOptionFromMember( MemberName = nameof( GetVersion ) )]
public class Program
{
    private readonly ISender _sender;


    /// <summary />
    public static int Main( string[] args )
    {
        /*
         * 
         */
        var sender = Sender.Smtp;

        var econfig = Environment.GetEnvironmentVariable( "EMAIL_SENDER" );

        if ( econfig != null )
        {
            try
            {
                sender = Enum.Parse<Sender>( econfig, true );
            }
            catch
            {
                Console.WriteLine( "err: sender '{0}' is unsupported", econfig );
                return 2;
            }
        }


        /*
         * 
         */
        var svc = new ServiceCollection();

        svc.AddOptions();

        // Smtp
        if ( sender == Sender.Smtp )
        {
            svc.AddOptions<MailkitSenderOptions>().ValidateOnStart();
            svc.AddTransient<IValidateOptions<MailkitSenderOptions>, MailkitSenderOptionsValidation>();

            svc.Configure<MailkitSenderOptions>( o =>
            {
                // Host
                o.Host = Environment.GetEnvironmentVariable( "SMTP_HOST" )!;

                // Port
                o.Port = 587;
                var port = Environment.GetEnvironmentVariable( "SMTP_PORT" );

                if ( port != null )
                    o.Port = int.Parse( port );

                // SSL
                o.UseSsl = true;
                var ssl = Environment.GetEnvironmentVariable( "SMTP_SSL" );

                if ( ssl != null )
                    o.UseSsl = bool.Parse( ssl );

                // Auth
                o.Username = Environment.GetEnvironmentVariable( "SMTP_USERNAME" );
                o.Password = Environment.GetEnvironmentVariable( "SMTP_PASSWORD" );
            } );
            svc.AddTransient<ISender, MailkitSender>();
        }

        // Resend
        if ( sender == Sender.Resend )
        {
            svc.AddOptions<ResendClientOptions>().ValidateOnStart();
            svc.AddTransient<IValidateOptions<ResendClientOptions>, ResendClientOptionsValidation>();

            svc.AddHttpClient<ResendClient>();
            svc.Configure<ResendClientOptions>( o =>
            {
                // Auth
                o.ApiToken = Environment.GetEnvironmentVariable( "RESEND_APITOKEN" )!;
            } );
            svc.AddTransient<IResend, ResendClient>();
            svc.AddTransient<ISender, ResendSender>();
        }

        var sp = svc.BuildServiceProvider();


        /*
         * 
         */
        var app = new CommandLineApplication<Program>();

        try
        {
            app.Conventions
                .UseDefaultConventions()
                .UseConstructorInjection( sp );
        }
        catch ( TargetInvocationException ex )
            when ( ex.InnerException?.GetType() == typeof( OptionsValidationException ) )
        {
            Console.WriteLine( "err: {0}", ex.InnerException.Message );

            return 2;
        }
        catch ( Exception ex )
        {
            Console.WriteLine( "ftl: unhandled exception during setup" );
            Console.WriteLine( ex.ToString() );

            return 2;
        }


        /*
         * 
         */
        try
        {
            return app.Execute( args );
        }
        catch ( UnrecognizedCommandParsingException ex )
        {
            Console.WriteLine( "err: " + ex.Message );

            return 2;
        }
        catch ( Exception ex )
        {
            Console.WriteLine( "ftl: unhandled exception during execution" );
            Console.WriteLine( ex.ToString() );

            return 2;
        }
    }


    /// <summary />
    private static string GetVersion()
    {
        return typeof( Program )
            .Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()!
            .InformationalVersion;
    }


    /// <summary />
    public Program( ISender sender )
    {
        _sender = sender;
    }


    /// <summary />
    [Argument( 0, Description = "Input JSON file" )]
    [FileExists]
    public string? InputFile { get; set; }


    /// <summary />
    public async Task<int> OnExecuteAsync( CommandLineApplication app )
    {
        /*
         * 
         */
        string json;

        if ( Console.IsInputRedirected == true && this.InputFile == null )
        {
            json = await Console.In.ReadToEndAsync();
        }
        else if ( this.InputFile != null )
        {
            json = File.ReadAllText( this.InputFile );
        }
        else
        {
            app.ShowHelp();
            return 1;
        }


        /*
         * 
         */
        Email message;

        var jso = new JsonSerializerOptions()
        {
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            PropertyNameCaseInsensitive = true,
        };

        try
        {
            message = JsonSerializer.Deserialize<Email>( json, jso )!;
        }
        catch ( Exception ex )
        {
            Console.Error.WriteLine( "err: SE001: invalid JSON" );
            Console.Error.WriteLine( ex.ToString() );

            return 1;
        }


        /*
         * Body
         */
        if ( message.HtmlBody?.StartsWith( "@" ) == true )
        {
            var fname = message.HtmlBody[ 1.. ];

            if ( File.Exists( fname ) == false )
            {
                Console.Error.WriteLine( "err: SE002: unable to load HTML body from file {0}", fname );
                return 1;
            }

            message.HtmlBody = await File.ReadAllTextAsync( fname );
        }

        if ( message.TextBody?.StartsWith( "@" ) == true )
        {
            var fname = message.TextBody[ 1.. ];

            if ( File.Exists( fname ) == false )
            {
                Console.Error.WriteLine( "err: SE003: unable to load text body from file {0}", fname );
                return 1;
            }

            message.TextBody = await File.ReadAllTextAsync( fname );
        }


        /*
         * Attachments
         */
        if ( message.Attachments?.Count > 0 )
        {
            foreach ( var att in message.Attachments )
            {
                att.Name = Path.GetFileName( att.Filename );

                if ( File.Exists( att.Filename ) == false )
                {
                    Console.Error.WriteLine( "err: SE004: unable to load attachment from file {0}", att.Filename );
                    return 1;
                }

                if ( att.ContentType?.StartsWith( "text/" ) == true
                    || att.ContentType?.StartsWith( "application/json" ) == true )
                {
                    att.TextContent = await File.ReadAllTextAsync( att.Filename );
                }
                else
                {
                    att.BinaryContent = await File.ReadAllBytesAsync( att.Filename );
                }
            }
        }


        /*
         * 
         */
        await _sender.SendAsync( message );

        return 0;
    }
}