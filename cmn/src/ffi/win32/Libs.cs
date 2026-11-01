// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

#pragma warning disable SA1307, S101, SA1202, S3903

/// <summary>
/// The Windows system library file names, so an interop declaration names a
/// library rather than repeating a string.
/// </summary>
[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:Field names should not contain underscore")]
[SuppressMessage("Critical Code Smell", "S3218:Inner class members should not shadow outer class \"static\" or type members")]
internal static partial class Interop
{
    /// <summary>
    /// The Win32 library file names used by the interop declarations in this
    /// assembly.
    /// </summary>
    internal static partial class Libs
    {
        /// <summary>The Active Directory Services interface library.</summary>
        internal const string Activeds = "activeds.dll";

        /// <summary>The Windows security and cryptography library.</summary>
        internal const string Advapi32 = "advapi32.dll";

        /// <summary>The authorization and role based access control library.</summary>
        internal const string Authz = "authz.dll";

        /// <summary>The Windows cryptography library.</summary>
        internal const string BCrypt = "BCrypt.dll";

        /// <summary>The credential user interface library.</summary>
        internal const string Credui = "credui.dll";

        /// <summary>The cryptography library.</summary>
        internal const string Crypt32 = "crypt32.dll";

        /// <summary>The cryptography user interface library.</summary>
        internal const string CryptUI = "cryptui.dll";

        /// <summary>The DNS client library.</summary>
        internal const string Dnsapi = "dnsapi.dll";

        /// <summary>The directory service role management library.</summary>
        internal const string Dsrole = "dsrole.dll";

        /// <summary>The graphics device interface library.</summary>
        internal const string Gdi32 = "gdi32.dll";

        /// <summary>The HTTP server API library.</summary>
        internal const string HttpApi = "httpapi.dll";

        /// <summary>The IP helper library.</summary>
        internal const string IpHlpApi = "iphlpapi.dll";

        /// <summary>The Windows kernel library, which is where the console and process APIs live.</summary>
        internal const string Kernel32 = "kernel32.dll";

        /// <summary>The network logon client library.</summary>
        internal const string Logoncli = "logoncli.dll";

        /// <summary>The Microsoft Winsock service provider library.</summary>
        internal const string Mswsock = "mswsock.dll";

        /// <summary>The Windows cryptography next generation library.</summary>
        internal const string NCrypt = "ncrypt.dll";

        /// <summary>The network management API library.</summary>
        internal const string Netapi32 = "netapi32.dll";

        /// <summary>The network management and diagnostics library.</summary>
        internal const string Netutils = "netutils.dll";

        /// <summary>The native API library, which is where the version query lives.</summary>
        internal const string NtDll = "ntdll.dll";

        /// <summary>The Open Database Connectivity library.</summary>
        internal const string Odbc32 = "odbc32.dll";

        /// <summary>The COM library.</summary>
        internal const string Ole32 = "ole32.dll";

        /// <summary>The COM automation library.</summary>
        internal const string OleAut32 = "oleaut32.dll";

        /// <summary>The performance data helper library.</summary>
        internal const string Pdh = "pdh.dll";

        /// <summary>The security support provider interface library.</summary>
        internal const string Secur32 = "secur32.dll";

        /// <summary>The Windows shell library.</summary>
        internal const string Shell32 = "shell32.dll";

        /// <summary>The security support provider client library.</summary>
        internal const string SspiCli = "sspicli.dll";

        /// <summary>The user interface library.</summary>
        internal const string User32 = "user32.dll";

        /// <summary>The version information library.</summary>
        internal const string Version = "version.dll";

        /// <summary>The WebSocket library.</summary>
        internal const string WebSocket = "websocket.dll";

        /// <summary>The Windows event tracing library.</summary>
        internal const string Wevtapi = "wevtapi.dll";

        /// <summary>The Windows HTTP services library.</summary>
        internal const string WinHttp = "winhttp.dll";

        /// <summary>The Windows multimedia library.</summary>
        internal const string WinMM = "winmm.dll";

        /// <summary>The workstation client library.</summary>
        internal const string Wkscli = "wkscli.dll";

        /// <summary>The Active Directory client library.</summary>
        internal const string Wldap32 = "wldap32.dll";

        /// <summary>The Winsock library.</summary>
        internal const string Ws2_32 = "ws2_32.dll";

        /// <summary>The terminal services library.</summary>
        internal const string Wtsapi32 = "wtsapi32.dll";

        /// <summary>The QUIC library.</summary>
        internal const string MsQuic = "msquic.dll";

        /// <summary>The .NET host policy library.</summary>
        internal const string HostPolicy = "hostpolicy.dll";

        /// <summary>The Universal C runtime library.</summary>
        internal const string Ucrtbase = "ucrtbase.dll";

        /// <summary>The OLE helper library.</summary>
        internal const string Xolehlp = "xolehlp.dll";
    }
}
