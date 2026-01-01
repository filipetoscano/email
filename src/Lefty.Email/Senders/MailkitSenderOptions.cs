namespace Lefty.Email.Senders;

/// <summary />
public class MailkitSenderOptions
{
    /// <summary />
    public string? Host { get; set; }

    /// <summary />
    public int Port { get; set; }

    /// <summary />
    public bool UseSsl { get; set; }


    /// <summary />
    public string? Username { get; set; }

    /// <summary />
    public string? Password { get; set; }

    /// <summary />
    public bool CheckCertificateRevocation { get; set; } = true;
}