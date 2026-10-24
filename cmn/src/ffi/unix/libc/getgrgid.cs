using System.Runtime.InteropServices;

#pragma warning disable CS0649, SA1025, SA1310

/// <summary>
/// The portable unix interop declarations, in the global namespace so a linking
/// project can drop them in without a name collision.
/// </summary>
internal static partial class FFI
{
    /// <summary>
    /// The C standard library calls this assembly uses, and the group database
    /// layout they return.
    /// </summary>
    internal static partial class Libc
    {
        /// <summary>Looks a group up by id, into a caller supplied buffer.</summary>
        /// <param name="uid">The group id to look up.</param>
        /// <param name="group">The record the call fills in.</param>
        /// <param name="buf">Caller supplied buffer for the record's strings.</param>
        /// <param name="bufLen">The size of <paramref name="buf"/> in bytes.</param>
        /// <param name="groupPtr">
        /// The record the platform actually used, which is a null pointer when there
        /// is no such group.
        /// </param>
        /// <returns>Zero on success, or an <see cref="FFI.Error"/> value.</returns>
        [LibraryImport(Libs.LibcName, StringMarshalling = StringMarshalling.Utf8, EntryPoint = "getgrgid_r", SetLastError = false)]
        internal static unsafe partial int getgrgidr(uint uid, out Group group, byte* buf, int bufLen, out IntPtr groupPtr);

        /// <summary>Looks a group up by name, into a caller supplied buffer.</summary>
        /// <param name="name">The group name to look up, as UTF-8.</param>
        /// <param name="group">The record the call fills in.</param>
        /// <param name="buf">Caller supplied buffer for the record's strings.</param>
        /// <param name="bufLen">The size of <paramref name="buf"/> in bytes.</param>
        /// <param name="groupPtr">
        /// The record the platform actually used, which is a null pointer when there
        /// is no such group.
        /// </param>
        /// <returns>Zero on success, or an <see cref="FFI.Error"/> value.</returns>
        [LibraryImport(Libs.LibcName, StringMarshalling = StringMarshalling.Utf8, EntryPoint = "getgrnam_r", SetLastError = false)]
        internal static unsafe partial int getgrnamr(string name, out Group group, byte* buf, int bufLen, out IntPtr groupPtr);

        /// <summary>
        /// Looks a group id up by name, doubling the buffer until the record fits.
        /// </summary>
        /// <param name="name">The group name to look up.</param>
        /// <returns>
        /// The group id, or <see langword="null"/> when there is no such group or the
        /// lookup failed.
        /// </returns>
        internal static unsafe uint? GetGroupId(string name)
        {
            var size = 512;
            var stackBuf = stackalloc byte[size];
            Group group;
            IntPtr groupPtr;
            int result = getgrnamr(name, out group, stackBuf, size, out groupPtr);
            if (result == 0)
            {
                if (groupPtr == IntPtr.Zero)
                {
                    // No such group
                    return null;
                }

                return group.GroupId;
            }

            while (true)
            {
                size *= 2;
                var buf = Marshal.AllocHGlobal(size);
                try
                {
                    result = getgrnamr(name, out group, (byte*)buf, size, out groupPtr);
                    if (result == 0)
                    {
                        if (groupPtr == IntPtr.Zero)
                        {
                            // No such group
                            return null;
                        }

                        return group.GroupId;
                    }

                    if (result == 0x10047)
                    {
                        continue;
                    }

                    return null; // EINVAL or other error
                }
                finally
                {
                    Marshal.FreeHGlobal(buf);
                }
            }
        }

        /// <summary>
        /// Looks a group name up by id, doubling the buffer until the record fits.
        /// </summary>
        /// <param name="groupId">The group id to look up.</param>
        /// <returns>
        /// The group name, or <see langword="null"/> when there is no such group or
        /// the lookup failed.
        /// </returns>
        internal static unsafe string? GetGroupName(uint groupId)
        {
            var size = 512;
            var stackBuf = stackalloc byte[size];
            Group group;
            IntPtr groupPtr;
            int result = getgrgidr(groupId, out group, stackBuf, size, out groupPtr);
            if (result == 0)
            {
                if (groupPtr == IntPtr.Zero)
                {
                    // No such group
                    return null;
                }

                return Marshal.PtrToStringUTF8((IntPtr)group.Name);
            }

            while (true)
            {
                size *= 2;
                var buf = Marshal.AllocHGlobal(size);
                try
                {
                    result = getgrgidr(groupId, out group, (byte*)buf, size, out groupPtr);
                    if (result == 0)
                    {
                        if (groupPtr == IntPtr.Zero)
                        {
                            // No such group
                            return null;
                        }

                        return Marshal.PtrToStringUTF8((IntPtr)group.Name);
                    }

                    if (result == 0x10047)
                    {
                        continue;
                    }

                    return null; // EINVAL or other error
                }
                finally
                {
                    Marshal.FreeHGlobal(buf);
                }
            }
        }

        /// <summary>
        /// The native <c>struct group</c>. The string fields are pointers into the
        /// caller's buffer, not managed strings.
        /// </summary>
        internal unsafe struct Group
        {
            /// <summary>
            /// A buffer size that fits a typical record. The lookups in this class
            /// start at 512 bytes rather than at this value, so this constant is not
            /// the one they use.
            /// </summary>
            internal const int InitialBufferSize = 256;

            /// <summary>The group name.</summary>
            internal byte* Name;

            /// <summary>The group password, which modern systems leave empty.</summary>
            internal byte* Password;

            /// <summary>The group id.</summary>
            internal uint GroupId;

            /// <summary>
            /// An array of byte pointers naming the members, terminated by a null
            /// pointer.
            /// </summary>
            internal IntPtr Members; // This is an array of byte pointers, representing group members
        }
    }
}
