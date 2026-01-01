namespace Lefty.Email;

/// <summary />
public interface ISender
{
    /// <summary />
    Task Send( Email message );
}