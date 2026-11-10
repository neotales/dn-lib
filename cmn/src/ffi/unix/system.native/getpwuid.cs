// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.InteropServices;
#pragma warning disable CS0649
#pragma warning disable S3903 // Types should be defined in named namespaces

/// <summary>
/// The portable unix interop declarations, in the global namespace so a linking
/// project can drop them in without a name collision.
/// </summary>
internal static partial class FFI
{
    /// <summary>
    /// The System.Native shim calls this assembly uses, and the password database
    /// layout they return.
    /// </summary>
    internal static partial class Sys
    {
        /// <summary>Looks a user up by id, into a caller supplied buffer.</summary>
        /// <param name="uid">The user id to look up.</param>
        /// <param name="pwd">The record the call fills in.</param>
        /// <param name="buf">Caller supplied buffer for the record's strings.</param>
        /// <param name="bufLen">The size of <paramref name="buf"/> in bytes.</param>
        /// <returns>Zero on success, or an <see cref="FFI.Error"/> value.</returns>
        [LibraryImport(Libs.SystemNative, StringMarshalling = StringMarshalling.Utf8, EntryPoint = "SystemNative_GetPwUidR", SetLastError = false)]
        internal static unsafe partial int GetPwUidR(uint uid, out Passwd pwd, byte* buf, int bufLen);

        /// <summary>Looks a user up by name, into a caller supplied buffer.</summary>
        /// <param name="name">The user name to look up, as UTF-8.</param>
        /// <param name="pwd">The record the call fills in.</param>
        /// <param name="buf">Caller supplied buffer for the record's strings.</param>
        /// <param name="bufLen">The size of <paramref name="buf"/> in bytes.</param>
        /// <returns>Zero on success, or an <see cref="FFI.Error"/> value.</returns>
        [LibraryImport(Libs.SystemNative, StringMarshalling = StringMarshalling.Utf8, EntryPoint = "SystemNative_GetPwNamR", SetLastError = false)]
        internal static unsafe partial int GetPwNamR(string name, out Passwd pwd, byte* buf, int bufLen);

        /// <summary>
        /// Looks a user id up by name, doubling the buffer until the record fits.
        /// </summary>
        /// <param name="name">The user name to look up.</param>
        /// <returns>
        /// The user id, or <see langword="null"/> when there is no such user or the
        /// lookup failed.
        /// </returns>
        internal static unsafe uint? GetUserId(string name)
        {
            var size = Passwd.InitialBufferSize;
            var stackBuf = stackalloc byte[size];
            Passwd pwd;
            int result = GetPwNamR(name, out pwd, stackBuf, size);
            if (result == 0)
            {
                return pwd.UserId;
            }

            while (true)
            {
                size *= 2;
                var buf = Marshal.AllocHGlobal(size);
                try
                {
                    result = GetPwNamR(name, out pwd, (byte*)buf, size);
                    if (result == 0)
                    {
                        return pwd.UserId;
                    }

                    if (result == (int)Error.ERANGE)
                    {
                        // The buffer was too small, try again with a larger one.
                        continue;
                    }

                    return null; // An error occurred, return null.
                }
                finally
                {
                    Marshal.FreeHGlobal(buf);
                }
            }
        }

        /// <summary>
        /// Looks a user name up by id, doubling the buffer until the record fits.
        /// </summary>
        /// <param name="uid">The user id to look up.</param>
        /// <returns>
        /// The user name, or <see langword="null"/> when there is no such user or the
        /// lookup failed.
        /// </returns>
        internal static unsafe string? GetUserName(uint uid)
        {
            var size = Passwd.InitialBufferSize;
            var stackBuf = stackalloc byte[size];
            Passwd pwd;
            int result = GetPwUidR(uid, out pwd, stackBuf, size);
            if (result == 0)
            {
                return Marshal.PtrToStringUTF8((IntPtr)pwd.Name);
            }

            while (true)
            {
                size *= 2;
                var buf = Marshal.AllocHGlobal(size);
                try
                {
                    result = GetPwUidR(uid, out pwd, (byte*)buf, size);
                    if (result == 0)
                    {
                        return Marshal.PtrToStringUTF8((IntPtr)pwd.Name);
                    }

                    if (result == (int)Error.ERANGE)
                    {
                        // The buffer was too small, try again with a larger one.
                        continue;
                    }

                    return null; // An error occurred, return null.
                }
                finally
                {
                    Marshal.FreeHGlobal(buf);
                }
            }
        }

#pragma warning disable S2344
#pragma warning disable S2346
#pragma warning disable SA1025
#pragma warning disable SA1310

        /// <summary>
        /// The native <c>struct passwd</c>. The string fields are pointers into the
        /// caller's buffer, not managed strings.
        /// </summary>
        internal unsafe struct Passwd
        {
            /// <summary>The buffer size to try first, which fits a typical record.</summary>
            internal const int InitialBufferSize = 256;

            /// <summary>The user name.</summary>
            internal byte* Name;

            /// <summary>The password hash, which modern systems leave empty.</summary>
            internal byte* Password;

            /// <summary>The user id.</summary>
            internal uint UserId;

            /// <summary>The primary group id.</summary>
            internal uint GroupId;

            /// <summary>The free text the user database holds, often the real name.</summary>
            internal byte* UserInfo;

            /// <summary>The home directory.</summary>
            internal byte* HomeDirectory;

            /// <summary>The login shell.</summary>
            internal byte* Shell;
        }
    }
}
