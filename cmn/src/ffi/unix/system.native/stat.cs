// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.InteropServices;

using Microsoft.Win32.SafeHandles;

#pragma warning disable IDE1006, SA1025, SA1310, CS8981, SA1300

/// <summary>
/// The portable unix interop declarations, in the global namespace so a linking
/// project can drop them in without a name collision.
/// </summary>
internal static partial class FFI
{
    /// <summary>
    /// The System.Native shim calls this assembly uses, and the <c>stat</c> layout
    /// they return.
    /// </summary>
    internal static partial class Sys
    {
        /// <summary>
        /// The fields of <see cref="FileStatus"/> the platform may or may not fill in.
        /// </summary>
        [Flags]
        internal enum FileStatusFlags
        {
            /// <summary>No optional field was filled in.</summary>
            None = 0,

            /// <summary>The birth time was filled in, which older kernels do not do.</summary>
            HasBirthTime = 1,
        }

        /// <summary>Reads the status of an already open file.</summary>
        /// <param name="fd">The handle to read.</param>
        /// <param name="output">The status the call returns.</param>
        /// <returns>Zero on success, or <c>-1</c> with the errno set.</returns>
        [LibraryImport(Libs.SystemNative, EntryPoint = "SystemNative_FStat", SetLastError = true)]
        internal static partial int fstat(SafeFileHandle fd, out FileStatus output);

        /// <summary>Reads the status of a path, following symbolic links.</summary>
        /// <param name="path">The path to read, as UTF-8.</param>
        /// <param name="output">The status the call returns.</param>
        /// <returns>Zero on success, or <c>-1</c> with the errno set.</returns>
        [LibraryImport(Libs.SystemNative, EntryPoint = "SystemNative_Stat", StringMarshalling = StringMarshalling.Utf8, SetLastError = true)]
        internal static partial int stat(string path, out FileStatus output);

        /// <summary>Reads the status of a path, without following a final symbolic link.</summary>
        /// <param name="path">The path to read, as UTF-8.</param>
        /// <param name="output">The status the call returns.</param>
        /// <returns>Zero on success, or <c>-1</c> with the errno set.</returns>
        [LibraryImport(Libs.SystemNative, EntryPoint = "SystemNative_LStat", StringMarshalling = StringMarshalling.Utf8, SetLastError = true)]
        internal static partial int lstat(string path, out FileStatus output);

        // Even though csc will by default use a sequential layout, a CS0649 warning as error
        // is produced for un-assigned fields when no StructLayout is specified.
        //
        // Explicitly saying Sequential disables that warning/error for consumers which only
        // use Stat in debug builds.

        /// <summary>
        /// The native <c>struct stat</c>, with the shim's extra fields appended.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct FileStatus
        {
            /// <summary>Which optional fields the platform filled in.</summary>
            internal FileStatusFlags Flags;

            /// <summary>The file type and permission bits.</summary>
            internal int Mode;

            /// <summary>The owning user id.</summary>
            internal uint Uid;

            /// <summary>The owning group id.</summary>
            internal uint Gid;

            /// <summary>The size in bytes.</summary>
            internal long Size;

            /// <summary>The last access time, in seconds since the unix epoch.</summary>
            internal long ATime;

            /// <summary>The nanosecond part of the last access time.</summary>
            internal long ATimeNsec;

            /// <summary>The last write time, in seconds since the unix epoch.</summary>
            internal long MTime;

            /// <summary>The nanosecond part of the last write time.</summary>
            internal long MTimeNsec;

            /// <summary>The last metadata change time, in seconds since the unix epoch.</summary>
            internal long CTime;

            /// <summary>The nanosecond part of the last metadata change time.</summary>
            internal long CTimeNsec;

            /// <summary>The creation time, in seconds since the unix epoch.</summary>
            internal long BirthTime;

            /// <summary>The nanosecond part of the creation time.</summary>
            internal long BirthTimeNsec;

            /// <summary>The device the entry lives on.</summary>
            internal long Dev;

            /// <summary>The inode number.</summary>
            internal long Ino;

            /// <summary>Platform specific user flags.</summary>
            internal uint UserFlags;
        }

        /// <summary>
        /// The file type bits of the mode field of <see cref="FileStatus"/>.
        /// </summary>
        internal static class FileTypes
        {
            /// <summary>The mask that selects the file type bits.</summary>
            internal const int S_IFMT = 0xF000;

            /// <summary>A named pipe.</summary>
            internal const int S_IFIFO = 0x1000;

            /// <summary>A character device.</summary>
            internal const int S_IFCHR = 0x2000;

            /// <summary>A directory.</summary>
            internal const int S_IFDIR = 0x4000;

            /// <summary>A regular file.</summary>
            internal const int S_IFREG = 0x8000;

            /// <summary>A symbolic link.</summary>
            internal const int S_IFLNK = 0xA000;

            /// <summary>A socket.</summary>
            internal const int S_IFSOCK = 0xC000;
        }
    }
}
