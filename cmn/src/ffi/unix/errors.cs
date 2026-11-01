// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

#pragma warning disable IDE1006, SA1025, SA1310, CS8981, SA1300

/// <summary>
/// The portable unix interop declarations, in the global namespace so a linking
/// project can drop them in without a name collision.
/// </summary>
internal static partial class FFI
{
    /// <summary>Common Unix errno error codes.</summary>
    // These values were defined in src/Native/System.Native/fxerrno.h
    //
    // They compare against values obtained via Interop.Sys.GetLastError() not Marshal.GetLastPInvokeError()
    // which obtains the raw errno that varies between unixes. The strong typing as an enum is meant to
    // prevent confusing the two. Casting to or from int is suspect. Use GetLastErrorInfo() if you need to
    // correlate these to the underlying platform values or obtain the corresponding error message.
    [SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1025:Code should not contain multiple whitespace in a row")]
    internal enum Error
    {
        /// <summary>The call succeeded, which is the only zero errno.</summary>
        SUCCESS = 0,

        /// <summary>Argument list too long.</summary>
        E2BIG = 0x10001,

        /// <summary>Permission denied.</summary>
        EACCES = 0x10002,

        /// <summary>Address in use.</summary>
        EADDRINUSE = 0x10003,

        /// <summary>Address not available.</summary>
        EADDRNOTAVAIL = 0x10004,

        /// <summary>Address family not supported.</summary>
        EAFNOSUPPORT = 0x10005,

        /// <summary>Resource unavailable, try again (same value as EWOULDBLOCK),.</summary>
        EAGAIN = 0x10006,

        /// <summary>Connection already in progress.</summary>
        EALREADY = 0x10007,

        /// <summary>Bad file descriptor.</summary>
        EBADF = 0x10008,

        /// <summary>Bad message.</summary>
        EBADMSG = 0x10009,

        /// <summary>Device or resource busy.</summary>
        EBUSY = 0x1000A,

        /// <summary>Operation canceled.</summary>
        ECANCELED = 0x1000B,

        /// <summary>No child processes.</summary>
        ECHILD = 0x1000C,

        /// <summary>Connection aborted.</summary>
        ECONNABORTED = 0x1000D,

        /// <summary>Connection refused.</summary>
        ECONNREFUSED = 0x1000E,

        /// <summary>Connection reset.</summary>
        ECONNRESET = 0x1000F,

        /// <summary>Resource deadlock would occur.</summary>
        EDEADLK = 0x10010,

        /// <summary>Destination address required.</summary>
        EDESTADDRREQ = 0x10011,

        /// <summary>Mathematics argument out of domain of function.</summary>
        EDOM = 0x10012,

        /// <summary>Reserved.</summary>
        EDQUOT = 0x10013,

        /// <summary>File exists.</summary>
        EEXIST = 0x10014,

        /// <summary>Bad address.</summary>
        EFAULT = 0x10015,

        /// <summary>File too large.</summary>
        EFBIG = 0x10016,

        /// <summary>Host is unreachable.</summary>
        EHOSTUNREACH = 0x10017,

        /// <summary>Identifier removed.</summary>
        EIDRM = 0x10018,

        /// <summary>Illegal byte sequence.</summary>
        EILSEQ = 0x10019,

        /// <summary>Operation in progress.</summary>
        EINPROGRESS = 0x1001A,

        /// <summary>Interrupted function.</summary>
        EINTR = 0x1001B,

        /// <summary>Invalid argument.</summary>
        EINVAL = 0x1001C,

        /// <summary>I/O error.</summary>
        EIO = 0x1001D,

        /// <summary>Socket is connected.</summary>
        EISCONN = 0x1001E,

        /// <summary>Is a directory.</summary>
        EISDIR = 0x1001F,

        /// <summary>Too many levels of symbolic links.</summary>
        ELOOP = 0x10020,

        /// <summary>File descriptor value too large.</summary>
        EMFILE = 0x10021,

        /// <summary>Too many links.</summary>
        EMLINK = 0x10022,

        /// <summary>Message too large.</summary>
        EMSGSIZE = 0x10023,

        /// <summary>Reserved.</summary>
        EMULTIHOP = 0x10024,

        /// <summary>Filename too long.</summary>
        ENAMETOOLONG = 0x10025,

        /// <summary>Network is down.</summary>
        ENETDOWN = 0x10026,

        /// <summary>Connection aborted by network.</summary>
        ENETRESET = 0x10027,

        /// <summary>Network unreachable.</summary>
        ENETUNREACH = 0x10028,

        /// <summary>Too many files open in system.</summary>
        ENFILE = 0x10029,

        /// <summary>No buffer space available.</summary>
        ENOBUFS = 0x1002A,

        /// <summary>No such device.</summary>
        ENODEV = 0x1002C,

        /// <summary>No such file or directory.</summary>
        ENOENT = 0x1002D,

        /// <summary>Executable file format error.</summary>
        ENOEXEC = 0x1002E,

        /// <summary>No locks available.</summary>
        ENOLCK = 0x1002F,

        /// <summary>Reserved.</summary>
        ENOLINK = 0x10030,

        /// <summary>Not enough space.</summary>
        ENOMEM = 0x10031,

        /// <summary>No message of the desired type.</summary>
        ENOMSG = 0x10032,

        /// <summary>Protocol not available.</summary>
        ENOPROTOOPT = 0x10033,

        /// <summary>No space left on device.</summary>
        ENOSPC = 0x10034,

        /// <summary>Function not supported.</summary>
        ENOSYS = 0x10037,

        /// <summary>The socket is not connected.</summary>
        ENOTCONN = 0x10038,

        /// <summary>Not a directory or a symbolic link to a directory.</summary>
        ENOTDIR = 0x10039,

        /// <summary>Directory not empty.</summary>
        ENOTEMPTY = 0x1003A,

        /// <summary>State not recoverable.</summary>
        ENOTRECOVERABLE = 0x1003B,

        /// <summary>Not a socket.</summary>
        ENOTSOCK = 0x1003C,

        /// <summary>Not supported (same value as EOPNOTSUP).</summary>
        ENOTSUP = 0x1003D,

        /// <summary>Inappropriate I/O control operation.</summary>
        ENOTTY = 0x1003E,

        /// <summary>No such device or address.</summary>
        ENXIO = 0x1003F,

        /// <summary>Value too large to be stored in data type.</summary>
        EOVERFLOW = 0x10040,

        /// <summary>Previous owner died.</summary>
        EOWNERDEAD = 0x10041,

        /// <summary>Operation not permitted.</summary>
        EPERM = 0x10042,

        /// <summary>Broken pipe.</summary>
        EPIPE = 0x10043,

        /// <summary>Protocol error.</summary>
        EPROTO = 0x10044,

        /// <summary>Protocol not supported.</summary>
        EPROTONOSUPPORT = 0x10045,

        /// <summary>Protocol wrong type for socket.</summary>
        EPROTOTYPE = 0x10046,

        /// <summary>Result too large.</summary>
        ERANGE = 0x10047,

        /// <summary>Read-only file system.</summary>
        EROFS = 0x10048,

        /// <summary>Invalid seek.</summary>
        ESPIPE = 0x10049,

        /// <summary>No such process.</summary>
        ESRCH = 0x1004A,

        /// <summary>Reserved.</summary>
        ESTALE = 0x1004B,

        /// <summary>Connection timed out.</summary>
        ETIMEDOUT = 0x1004D,

        /// <summary>Text file busy.</summary>
        ETXTBSY = 0x1004E,

        /// <summary>Cross-device link.</summary>
        EXDEV = 0x1004F,

        /// <summary>Socket type not supported.</summary>
        ESOCKTNOSUPPORT = 0x1005E,

        /// <summary>Protocol family not supported.</summary>
        EPFNOSUPPORT = 0x10060,

        /// <summary>Socket shutdown.</summary>
        ESHUTDOWN = 0x1006C,

        /// <summary>Host is down.</summary>
        EHOSTDOWN = 0x10070,

        /// <summary>No data available.</summary>
        ENODATA = 0x10071,

        // Custom Error codes to track errors beyond kernel interface.

        /// <summary>Name lookup failed.</summary>
        EHOSTNOTFOUND = 0x20001,

        /// <summary>Unspecified socket error.</summary>
        ESOCKETERROR = 0x20002,

        // POSIX permits these to have the same value and we make them always equal so
        // that we do not introduce a dependency on distinguishing between them that
        // would not work on all platforms.

        /// <summary>Operation not supported on socket.</summary>
        EOPNOTSUPP = ENOTSUP,

        /// <summary>Operation would block.</summary>
        EWOULDBLOCK = EAGAIN,
    }

    /// <summary>
    /// A platform agnostic <see cref="Error"/> paired with the platform specific
    /// errno it came from, so a caller can have both without converting twice.
    /// </summary>
    internal readonly struct ErrorInfo
    {
        private readonly Error error;
        private readonly int rawErrno;

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorInfo"/> struct from a
        /// platform errno.
        /// </summary>
        /// <param name="errno">The platform specific errno value.</param>
        internal ErrorInfo(int errno)
        {
            this.error = Sys.ConvertErrorPlatformToPal(errno);
            this.rawErrno = errno;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorInfo"/> struct from an
        /// already converted error.
        /// </summary>
        /// <param name="error">The error, which is converted to a platform errno on first use.</param>
        internal ErrorInfo(Error error)
        {
            this.error = error;
            this.rawErrno = -1;
        }

        /// <summary>
        /// Gets the platform agnostic error.
        /// </summary>
        internal Error Error
        {
            get
            {
                return this.error;
            }
        }

        /// <summary>
        /// Gets the platform specific errno, converting the portable error on first
        /// read.
        /// </summary>
        internal int RawErrno
        {
            get
            {
                return this.rawErrno == -1 ? Sys.ConvertErrorPalToPlatform(this.error) : this.rawErrno;
            }
        }

        /// <summary>
        /// Returns a debugging string naming the errno, the error and its message.
        /// </summary>
        /// <returns>
        /// A string built from the member names. There is no localisation, because the
        /// text is for a log line rather than for a user.
        /// </returns>
        public override string ToString()
        {
            return $"RawErrno: {this.RawErrno} Error: {this.Error} GetErrorMessage: {this.GetErrorMessage()}";
        }

        /// <summary>
        /// Gets the platform's own message for the errno.
        /// </summary>
        /// <returns>The message <c>strerror</c> gives for the raw errno.</returns>
        internal string GetErrorMessage()
        {
            return Sys.StrError(this.RawErrno);
        }
    }

    /// <summary>
    /// The errno helpers: reading the last error, converting between the platform
    /// and the portable numbering, and formatting a message.
    /// </summary>
    internal static partial class Sys
    {
        /// <summary>
        /// Reads the errno of the last failed platform call.
        /// </summary>
        /// <returns>The last error, converted to the portable numbering.</returns>
        internal static Error GetLastError()
        {
            return ConvertErrorPlatformToPal(Marshal.GetLastPInvokeError());
        }

        /// <summary>
        /// Reads the errno of the last failed platform call, keeping the raw value too.
        /// </summary>
        /// <returns>An <see cref="ErrorInfo"/> holding both numbers.</returns>
        internal static ErrorInfo GetLastErrorInfo()
        {
            return new ErrorInfo(Marshal.GetLastPInvokeError());
        }

        /// <summary>
        /// Formats the platform's message for an errno.
        /// </summary>
        /// <param name="platformErrno">The platform specific errno value.</param>
        /// <returns>
        /// The message for that errno. A message that does not fit the buffer is
        /// truncated rather than retried, because the buffer is large enough for
        /// every errno this library can produce.
        /// </returns>
        internal static unsafe string StrError(int platformErrno)
        {
            const int MaxBufferLength = 1024; // should be long enough for most any UNIX error
            byte* buffer = stackalloc byte[MaxBufferLength];
            byte* message = StrErrorR(platformErrno, buffer, MaxBufferLength);

            if (message == null)
            {
                // This means the buffer was not large enough, but still contains
                // as much of the error message as possible and is guaranteed to
                // be null-terminated. We're not currently resizing/retrying because
                // MaxBufferLength is large enough in practice, but we could do
                // so here in the future if necessary.
                message = buffer;
            }

            return Marshal.PtrToStringUTF8((IntPtr)message)!;
        }

#if SERIAL_PORTS
        /// <summary>Converts a platform errno to the portable numbering.</summary>
        /// <param name="platformErrno">The platform specific errno value.</param>
        /// <returns>The matching <see cref="FFI.Error"/>.</returns>
        [LibraryImport(libs.IOPortsNative, EntryPoint = "SystemIoPortsNative_ConvertErrorPlatformToPal")]
        internal static partial Error ConvertErrorPlatformToPal(int platformErrno);

        /// <summary>Converts a portable error back to a platform errno.</summary>
        /// <param name="error">The portable error.</param>
        /// <returns>The platform specific errno value.</returns>
        [LibraryImport(libs.IOPortsNative, EntryPoint = "SystemIoPortsNative_ConvertErrorPalToPlatform")]
        internal static partial int ConvertErrorPalToPlatform(Error error);

        /// <summary>The reentrant form of <c>strerror</c>.</summary>
        /// <param name="platformErrno">The platform specific errno value.</param>
        /// <param name="buffer">Caller supplied buffer for the message.</param>
        /// <param name="bufferSize">The size of <paramref name="buffer"/> in bytes.</param>
        /// <returns>
        /// A pointer to the message, which is <paramref name="buffer"/> itself when the
        /// message did not fit.
        /// </returns>
        [LibraryImport(libs.IOPortsNative, EntryPoint = "SystemIoPortsNative_StrErrorR")]
        private static unsafe partial byte* StrErrorR(int platformErrno, byte* buffer, int bufferSize);
#else
        /// <summary>Converts a platform errno to the portable numbering.</summary>
        /// <param name="platformErrno">The platform specific errno value.</param>
        /// <returns>The matching <see cref="FFI.Error"/>.</returns>
        [LibraryImport(Libs.SystemNative, EntryPoint = "SystemNative_ConvertErrorPlatformToPal")]
        [SuppressGCTransition]
        internal static partial Error ConvertErrorPlatformToPal(int platformErrno);

        /// <summary>Converts a portable error back to a platform errno.</summary>
        /// <param name="error">The portable error.</param>
        /// <returns>The platform specific errno value.</returns>
        [LibraryImport(Libs.SystemNative, EntryPoint = "SystemNative_ConvertErrorPalToPlatform")]
        [SuppressGCTransition]
        internal static partial int ConvertErrorPalToPlatform(Error error);

        /// <summary>The reentrant form of <c>strerror</c>.</summary>
        /// <param name="platformErrno">The platform specific errno value.</param>
        /// <param name="buffer">Caller supplied buffer for the message.</param>
        /// <param name="bufferSize">The size of <paramref name="buffer"/> in bytes.</param>
        /// <returns>
        /// A pointer to the message, which is <paramref name="buffer"/> itself when the
        /// message did not fit.
        /// </returns>
        [LibraryImport(Libs.SystemNative, EntryPoint = "SystemNative_StrErrorR")]
        private static unsafe partial byte* StrErrorR(int platformErrno, byte* buffer, int bufferSize);
#endif
    }
}

/// <summary>
/// The <see cref="FFI.ErrorInfo"/> shorthand, which is an extension method because
/// an extension method cannot be nested inside the <c>FFI</c> class.
/// </summary>
/// <remarks>
/// The intended use is <c>FFI.Error.EPERM.Info()</c>, which is shorter than
/// <c>new FFI.ErrorInfo(FFI.Error.EPERM)</c>. An error that came from the system
/// should be read with <c>GetLastErrorInfo()</c> rather than with
/// <c>GetLastError().Info()</c>: the latter converts twice, which is both wasteful
/// and lossy for a raw errno that has no equivalent in the enum.
/// </remarks>
[SuppressMessage("Major Bug", "S3903:Types should be defined in named namespaces")]
internal static class InteropErrorExtensions
{
    /// <summary>Pairs an error with its platform errno, without reading the system.</summary>
    /// <param name="error">The portable error.</param>
    /// <returns>An <see cref="FFI.ErrorInfo"/> for that error.</returns>
    public static FFI.ErrorInfo Info(this FFI.Error error)
    {
        return new FFI.ErrorInfo(error);
    }
}
