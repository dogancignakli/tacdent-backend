using System.Net;
using System.Net.Sockets;

namespace Tacdent.Api.Extensions;

public static class HttpContextExtensions
{
    /// <summary>
    /// Returns the client IP after <see cref="Microsoft.AspNetCore.HttpOverrides.ForwardedHeadersMiddleware"/>
    /// has processed trusted proxy headers. Does not read raw client-supplied forwarding headers.
    /// </summary>
    public static string? GetClientIpAddress(this HttpContext context)
    {
        var remote = context.Connection.RemoteIpAddress;
        if (remote is null)
        {
            return null;
        }

        if (IPAddress.IsLoopback(remote))
        {
            return remote.AddressFamily == AddressFamily.InterNetworkV6 ? "::1" : "127.0.0.1";
        }

        if (remote.IsIPv4MappedToIPv6)
        {
            return remote.MapToIPv4().ToString();
        }

        return remote.ToString();
    }
}
