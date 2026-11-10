using System.Diagnostics.CodeAnalysis;

#pragma warning disable IDE1006, SA1025, SA1310, CS8981, SA1300

[assembly: SuppressMessage("", "CS8981", Justification = "cause")]

/// <summary>
/// The portable unix interop declarations, in the global namespace so a linking
/// project can drop them in without a name collision.
/// </summary>
internal static partial class FFI
{
    /// <summary>
    /// The unix library names the interop declarations in this assembly bind to.
    /// </summary>
    internal static partial class Libs
    {
        /// <summary>The C standard library, which holds the file system calls.</summary>
        internal const string LibcName = "libc";

        // Shims

        /// <summary>
        /// The System.Native shim, which is where the calls .NET needs but unix does
        /// not provide in a portable form are implemented.
        /// </summary>
        internal const string SystemNative = "System.Native";

        /// <summary>The System.Net.Http.Native shim.</summary>
        internal const string HttpNative = "System.Net.Http.Native";

        /// <summary>The System.Security.Cryptography.Native shim.</summary>
        internal const string CryptoNative = "System.Security.Cryptography.Native";
    }
}
