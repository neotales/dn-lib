#pragma warning disable S3903

/// <summary>
/// Windows library file names used by the shared interop declarations.
/// </summary>
internal static partial class Interop
{
    /// <summary>The Win32 library file names.</summary>
    internal static partial class Libs
    {
        /// <summary>The base Windows API library.</summary>
        internal const string Kernel32 = "kernel32.dll";

        /// <summary>The Windows shell library.</summary>
        internal const string Shell32 = "shell32.dll";
    }
}
