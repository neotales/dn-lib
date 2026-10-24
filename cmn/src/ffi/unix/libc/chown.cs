using System.Runtime.InteropServices;
#pragma warning disable S3903 // Types should be defined in named namespaces

/// <summary>
/// The portable unix interop declarations, in the global namespace so a linking
/// project can drop them in without a name collision.
/// </summary>
internal static partial class FFI
{
    /// <summary>
    /// The C standard library calls this assembly uses.
    /// </summary>
    internal static partial class Libc
    {
        /// <summary>Changes the owner or the group of a file or directory.</summary>
        /// <param name="path">The path to change, as UTF-8.</param>
        /// <param name="owner">The new owning user id, or <c>-1</c> to leave it alone.</param>
        /// <param name="group">The new owning group id, or <c>-1</c> to leave it alone.</param>
        /// <returns>Zero on success, or <c>-1</c> with the errno set.</returns>
        [LibraryImport(Libs.LibcName, EntryPoint = "chown", StringMarshalling = StringMarshalling.Utf8, SetLastError = true)]
        internal static partial int chown(string path, uint owner, uint group);

        /// <summary>
        /// Changes the owner or the group of a symbolic link itself rather than of
        /// what it points at.
        /// </summary>
        /// <param name="path">The link to change, as UTF-8.</param>
        /// <param name="owner">The new owning user id, or <c>-1</c> to leave it alone.</param>
        /// <param name="group">The new owning group id, or <c>-1</c> to leave it alone.</param>
        /// <returns>Zero on success, or <c>-1</c> with the errno set.</returns>
        [LibraryImport(Libs.LibcName, EntryPoint = "lchown", StringMarshalling = StringMarshalling.Utf8, SetLastError = true)]
        internal static partial int lchown(string path, uint owner, uint group);
    }
}
