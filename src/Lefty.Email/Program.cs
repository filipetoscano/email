using Lefty.Email.Senders;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Resend;
using Spectre.Console;
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
                AnsiConsole.MarkupLineInterpolated( $"[red]err[/]: $env:EMAIL_SENDER value '{econfig}' is unsupported" );
                return 2;
            }
        }


        /*
         * 
         */
        var svc = new ServiceCollection();

        svc.AddOptions();

        // Null
        if ( sender == Sender.Null )
        {
            svc.AddTransient<ISender, NullSender>();
        }

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
                {
                    try
                    {
                        o.Port = int.Parse( port );
                    }
                    catch
                    {
                        o.Port = -1;
                    }
                }

                // SSL
                o.UseSsl = true;
                var ssl = Environment.GetEnvironmentVariable( "SMTP_SSL" );

                if ( ssl != null )
                {
                    try
                    {
                        o.UseSsl = bool.Parse( ssl );
                    }
                    catch
                    {
                    }
                }

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
            AnsiConsole.MarkupLineInterpolated( $"[red]err[/]: {ex.InnerException.Message}" );
            return 2;
        }
        catch ( Exception ex )
        {
            AnsiConsole.MarkupLineInterpolated( $"[red]ftl[/]: unhandled exception during setup" );
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
            AnsiConsole.MarkupLineInterpolated( $"[red]err[/]: {ex.Message}" );

            return 2;
        }
        catch ( Exception ex )
        {
            AnsiConsole.MarkupLineInterpolated( $"[red]ftl[/]: unhandled exception during execution" );
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
    [Option( "-f|--from", CommandOptionType.SingleValue, Description = "Sender email address" )]
    public string? FromAddress { get; set; }

    /// <summary />
    [Option( "-t|--to", CommandOptionType.MultipleValue, Description = "Recipient email address" )]
    public List<string>? ToAddress { get; set; }

    /// <summary />
    [Option( "-s|--subject", CommandOptionType.SingleValue, Description = "Subject" )]
    public string? Subject { get; set; }

    /// <summary />
    [Option( "-h|--html-file", CommandOptionType.SingleValue, Description = "HTML file" )]
    [FileExists]
    public string? HtmlFile { get; set; }

    /// <summary />
    [Option( "-x|--text-file", CommandOptionType.SingleValue, Description = "Text file" )]
    [FileExists]
    public string? TextFile { get; set; }

    /// <summary />
    [Option( "-X|--text", CommandOptionType.SingleValue, Description = "Text content" )]
    public string? Text { get; set; }


    /// <summary />
    [Option( "-e|--env", CommandOptionType.NoValue, Description = "Load sender/recipient from environment variables" )]
    public bool FromEnvironment { get; set; }


    /// <summary />
    public async Task<int> OnExecuteAsync( CommandLineApplication app )
    {
        /*
         * 
         */
        string cwd = Environment.CurrentDirectory;
        string? json = null;

        if ( Console.IsInputRedirected == true )
        {
            json = await Console.In.ReadToEndAsync();
        }
        else if ( this.InputFile != null )
        {
            json = File.ReadAllText( this.InputFile );
            cwd = Path.GetDirectoryName( this.InputFile )!;
        }


        /*
         * 
         */
        Email message;

        if ( json != null )
        {
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
                AnsiConsole.MarkupLineInterpolated( $"[red]err[/]: file is not valid JSON" );
                Console.Error.WriteLine( ex.ToString() );

                return 1;
            }
        }
        else
        {
            message = new Email();
        }


        /*
         * From environment
         */
        if ( this.FromEnvironment == true )
        {
            // Sender
            var from = Environment.GetEnvironmentVariable( "EMAIL_FROM" );

            if ( string.IsNullOrEmpty( from ) == false )
                message.From = from;


            // Recipient
            var to = Environment.GetEnvironmentVariable( "EMAIL_TO" );

            if ( string.IsNullOrEmpty( to ) == false )
            {
                var parts = to.Split( ';' );
                message.To = EmailAddressList.From( parts );
            }


            // Subject
            var subject = Environment.GetEnvironmentVariable( "EMAIL_SUBJECT" );

            if ( string.IsNullOrEmpty( subject ) == false )
                message.Subject = subject;


            // HTML body
            var html = Environment.GetEnvironmentVariable( "EMAIL_HTML" );

            if ( string.IsNullOrEmpty( html ) == false )
                message.HtmlBody = "@" + html;


            // Text body
            var text = Environment.GetEnvironmentVariable( "EMAIL_TEXT" );

            if ( string.IsNullOrEmpty( text ) == false )
                message.TextBody = "@" + text;
        }


        /*
         * From command-line
         */
        if ( string.IsNullOrEmpty( this.FromAddress ) == false )
            message.From = this.FromAddress;

        if ( this.ToAddress != null )
            message.To = EmailAddressList.From( this.ToAddress );

        if ( string.IsNullOrEmpty( this.Subject ) == false )
            message.Subject = this.Subject;

        if ( this.HtmlFile != null )
            message.HtmlBody = await File.ReadAllTextAsync( this.HtmlFile );

        if ( this.TextFile != null )
            message.TextBody = await File.ReadAllTextAsync( this.TextFile );

        if ( string.IsNullOrEmpty( this.Text ) == false )
            message.TextBody = this.Text;


        /*
         * Body
         */
        if ( message.HtmlBody?.StartsWith( "@" ) == true )
        {
            var fname = Path.Combine( cwd, message.HtmlBody[ 1.. ] );

            if ( File.Exists( fname ) == false )
            {
                AnsiConsole.MarkupLineInterpolated( $"[red]err[/]: unable to load HTML body from file '{fname}'" );
                return 1;
            }

            message.HtmlBody = await File.ReadAllTextAsync( fname );
        }

        if ( message.TextBody?.StartsWith( "@" ) == true )
        {
            var fname = Path.Combine( cwd, message.TextBody[ 1.. ] );

            if ( File.Exists( fname ) == false )
            {
                AnsiConsole.MarkupLineInterpolated( $"[red]err[/]: unable to load text body from file '{fname}'" );
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
                var fname = Path.Combine( cwd, att.Filename );

                if ( File.Exists( fname ) == false )
                {
                    AnsiConsole.MarkupLineInterpolated( $"[red]err[/]: unable to load attachment from file '{fname}'" );
                    return 1;
                }

                att.Name = Path.GetFileName( att.Filename );
                att.BinaryContent = await File.ReadAllBytesAsync( fname );
            }
        }


        /*
         * Validations
         */
        if ( message.From == null )
        {
            AnsiConsole.MarkupLineInterpolated( $"[red]err[/]: message has no sender/from" );
            return 1;
        }

        if ( message.To == null )
        {
            AnsiConsole.MarkupLineInterpolated( $"[red]err[/]: message has no recipient/to" );
            return 1;
        }

        if ( message.To.Count() == 0 )
        {
            AnsiConsole.MarkupLineInterpolated( $"[red]err[/]: message has no recipient/to" );
            return 1;
        }

        if ( message.Subject == null )
        {
            AnsiConsole.MarkupLineInterpolated( $"[red]err[/]: message has no subject" );
            return 1;
        }

        if ( message.HtmlBody == null && message.TextBody == null )
        {
            AnsiConsole.MarkupLineInterpolated( $"[red]err[/]: message has neither html/text body" );
            return 1;
        }


        /*
         * 
         */
        var output = await _sender.SendAsync( message );

        AnsiConsole.MarkupLineInterpolated( $"[green]ok[/]: {output}" );

        return 0;
    }
}