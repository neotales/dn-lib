using System.Runtime.InteropServices;

#pragma warning disable CS0649, SA1025, SA1300, SA1310, S3903

/// <summary>
/// Unix file status, read through libc rather than through a runtime shim.
/// </summary>
internal static partial class FFI
{
    /// <summary>The status calls and the managed record they fill.</summary>
    internal static partial class Sys
    {
        /// <summary>Reads the status of a path, following a final symbolic link.</summary>
        /// <param name="path">The path to read.</param>
        /// <param name="output">The status the call returns.</param>
        /// <returns>Zero on success, or <c>-1</c> with the errno set.</returns>
        internal static int stat(string path, out FileStatus output)
        {
            return Read(path, follow: true, out output);
        }

        /// <summary>Reads the status of a path, without following a final symbolic link.</summary>
        /// <param name="path">The path to read.</param>
        /// <param name="output">The status the call returns.</param>
        /// <returns>Zero on success, or <c>-1</c> with the errno set.</returns>
        internal static int lstat(string path, out FileStatus output)
        {
            return Read(path, follow: false, out output);
        }

        /// <summary>
        /// The file type and permission bits, ownership, size, times, device, and inode.
        /// </summary>
        internal struct FileStatus
        {
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

            /// <summary>The last write time, in seconds since the unix epoch.</summary>
            internal long MTime;

            /// <summary>The last metadata change time, in seconds since the unix epoch.</summary>
            internal long CTime;

            /// <summary>The creation time, when the platform has one. Otherwise zero.</summary>
            internal long BirthTime;

            /// <summary>The device the entry lives on.</summary>
            internal long Dev;

            /// <summary>The inode number.</summary>
            internal long Ino;
        }

        private static int Read(string path, bool follow, out FileStatus output)
        {
            output = default;
            int rc = follow ? LibcStat(path, out NativeStat native) : LibcLStat(path, out native);
            if (rc != 0)
            {
                return -1;
            }

            output = new FileStatus
            {
                Mode = native.Mode,
                Uid = native.Uid,
                Gid = native.Gid,
                Size = native.Size,
                ATime = native.AccessSeconds,
                MTime = native.ModifySeconds,
                CTime = native.ChangeSeconds,
                BirthTime = 0,
                Dev = native.Dev,
                Ino = native.Ino,
            };
            return 0;
        }

        // glibc's struct stat on linux-x64. Field order is the public ABI, not a
        // runtime shim. Times are timespec: seconds then nanoseconds.
        [StructLayout(LayoutKind.Sequential)]
        private struct NativeStat
        {
            internal long Dev;
            internal long Ino;
            internal long NLink;
            internal int Mode;
            internal uint Uid;
            internal uint Gid;
            internal int Pad0;
            internal long RDev;
            internal long Size;
            internal long BlockSize;
            internal long Blocks;
            internal long AccessSeconds;
            internal long AccessNanos;
            internal long ModifySeconds;
            internal long ModifyNanos;
            internal long ChangeSeconds;
            internal long ChangeNanos;
            internal long Reserved0;
            internal long Reserved1;
            internal long Reserved2;
        }

        [LibraryImport(Libs.LibcName, EntryPoint = "stat", StringMarshalling = StringMarshalling.Utf8, SetLastError = true)]
        private static partial int LibcStat(string path, out NativeStat output);

        [LibraryImport(Libs.LibcName, EntryPoint = "lstat", StringMarshalling = StringMarshalling.Utf8, SetLastError = true)]
        private static partial int LibcLStat(string path, out NativeStat output);
    }
}
