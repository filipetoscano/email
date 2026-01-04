using System.Text.Json;

namespace Lefty.Email.Senders;

/// <summary />
public class NullSender : ISender
{
    /// <summary />
    public async Task<string> SendAsync( Email message )
    {
        await Task.Yield();

        var jso = new JsonSerializerOptions()
        {
            WriteIndented = true,
            IndentSize = 2,
        };

        return JsonSerializer.Serialize( message, jso )!;
    }
}