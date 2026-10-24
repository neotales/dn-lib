// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;

/// <summary>
/// The kernel32 standard handle binding, which is how a process finds the
/// standard input, output and error handles.
/// </summary>
internal static partial class Interop
{
    /// <summary>
    /// The standard handle binding, which lives in kernel32 rather than in the
    /// process's own start up data.
    /// </summary>
    internal static partial class Kernel32
    {
        /// <summary>Retrieves a handle for one of the standard streams.</summary>
        /// <param name="nStdHandle">
        /// Which stream to ask for, as a <see cref="Interop.Kernel32.STD_OUTPUT_HANDLE"/>,
        /// <see cref="Interop.Kernel32.STD_INPUT_HANDLE"/> or
        /// <see cref="Interop.Kernel32.STD_ERROR_HANDLE"/> value.
        /// </param>
        /// <returns>
        /// The handle for that stream, which is a real console handle only when the
        /// stream has not been redirected; otherwise it is a file or pipe handle, or
        /// <see cref="IntPtr.Zero"/> when the stream is not valid.
        /// </returns>
        [SuppressGCTransition]
        [LibraryImport(Libs.Kernel32)]
        internal static partial IntPtr GetStdHandle(int nStdHandle);
    }
}
