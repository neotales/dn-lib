using System.Runtime.InteropServices;

#pragma warning disable S3903

/// <summary>
/// The error text for a failed libc call.
/// </summary>
internal static partial class FFI
{
    /// <summary>Error helpers for the unix calls in this assembly.</summary>
    internal static partial class Sys
    {
        /// <summary>The text for a platform errno, or for <c>-1</c> when the errno was just set.</summary>
        /// <param name="platformErrno">The errno, or <c>-1</c> to read the last p/invoke error.</param>
        /// <returns>The platform's error text, or a numeric fallback.</returns>
        internal static string StrError(int platformErrno)
        {
            int errno = platformErrno < 0 ? Marshal.GetLastPInvokeError() : platformErrno;
            nint message = NativeStrError(errno);
            return Marshal.PtrToStringUTF8(message) ?? $"errno {errno}";
        }

        [LibraryImport(Libs.LibcName, EntryPoint = "strerror")]
        private static partial nint NativeStrError(int errnum);
    }
}
