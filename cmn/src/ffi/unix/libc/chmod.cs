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
        /// <summary>Changes the permission bits of a file or directory.</summary>
        /// <param name="path">The path to change, as UTF-8.</param>
        /// <param name="mode">The new permission and mode bits.</param>
        /// <returns>Zero on success, or <c>-1</c> with the errno set.</returns>
        [LibraryImport(Libs.LibcName, EntryPoint = "chmod", StringMarshalling = StringMarshalling.Utf8, SetLastError = true)]
        internal static partial int chmod(string path, int mode);
    }
}
