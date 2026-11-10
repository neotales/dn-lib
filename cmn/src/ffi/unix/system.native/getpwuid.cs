using System.Runtime.InteropServices;

#pragma warning disable CS0649, SA1025, SA1310, S3903

/// <summary>
/// User-database lookups through libc.
/// </summary>
internal static partial class FFI
{
    /// <summary>The user-database calls this assembly uses.</summary>
    internal static partial class Sys
    {
        /// <summary>Looks a user id up by name.</summary>
        /// <param name="name">The user name to look up.</param>
        /// <returns>The user id, or <see langword="null"/> when there is no such user.</returns>
        internal static unsafe uint? GetUserId(string name)
        {
            if (!TryLookup(byName: true, name, uid: 0, out Passwd passwd, out nint record))
            {
                return null;
            }

            return record == 0 ? null : passwd.Uid;
        }

        /// <summary>Looks a user name up by id.</summary>
        /// <param name="uid">The user id to look up.</param>
        /// <returns>The user name, or <see langword="null"/> when there is no such user.</returns>
        internal static unsafe string? GetUserName(uint uid)
        {
            if (!TryLookup(byName: false, name: string.Empty, uid, out Passwd passwd, out nint record) || record == 0)
            {
                return null;
            }

            return Marshal.PtrToStringUTF8((nint)passwd.Name);
        }

        private static unsafe bool TryLookup(bool byName, string name, uint uid, out Passwd passwd, out nint record)
        {
            const int Erange = 34;
            var size = 512;
            var stack = stackalloc byte[size];
            int result = byName
                ? GetPwNamR(name, out passwd, stack, size, out record)
                : GetPwUidR(uid, out passwd, stack, size, out record);
            if (result == 0)
            {
                return true;
            }

            while (result == Erange)
            {
                size *= 2;
                var buf = Marshal.AllocHGlobal(size);
                try
                {
                    result = byName
                        ? GetPwNamR(name, out passwd, (byte*)buf, size, out record)
                        : GetPwUidR(uid, out passwd, (byte*)buf, size, out record);
                    if (result == 0)
                    {
                        return true;
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(buf);
                }
            }

            passwd = default;
            record = 0;
            return false;
        }

        [LibraryImport(Libs.LibcName, EntryPoint = "getpwuid_r", StringMarshalling = StringMarshalling.Utf8)]
        private static unsafe partial int GetPwUidR(uint uid, out Passwd pwd, byte* buf, int bufLen, out nint record);

        [LibraryImport(Libs.LibcName, EntryPoint = "getpwnam_r", StringMarshalling = StringMarshalling.Utf8)]
        private static unsafe partial int GetPwNamR(string name, out Passwd pwd, byte* buf, int bufLen, out nint record);

        [StructLayout(LayoutKind.Sequential)]
        private unsafe struct Passwd
        {
            internal byte* Name;
            internal byte* Password;
            internal uint Uid;
            internal uint Gid;
            internal byte* Gecos;
            internal byte* Directory;
            internal byte* Shell;
        }
    }
}
