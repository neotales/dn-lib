// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

/// <summary>
/// The kernel32 console mode bindings: reading and writing the mode of a console
/// handle, which is how virtual terminal processing gets turned on.
/// </summary>
[SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:Elements should appear in the correct order")]
[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:Field names should not contain underscore")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
internal static partial class Interop
{
    /// <summary>
    /// The console handle values, the console mode flags and the two functions
    /// that read and write a console mode.
    /// </summary>
    internal static partial class Kernel32
    {
        /// <summary>The standard output handle value.</summary>
        internal const int STD_OUTPUT_HANDLE = -11;

        /// <summary>The standard input handle value.</summary>
        internal const int STD_INPUT_HANDLE = -10;

        /// <summary>The standard error handle value.</summary>
        internal const int STD_ERROR_HANDLE = -12;

        /// <summary>Input mode: characters are processed by the read handler.</summary>
        internal const int ENABLE_PROCESSED_INPUT = 0x0001;

        /// <summary>Input mode: input is read a line at a time.</summary>
        internal const int ENABLE_LINE_INPUT = 0x0002;

        /// <summary>Input mode: characters are echoed as they are read.</summary>
        internal const int ENABLE_ECHO_INPUT = 0x0004;

        /// <summary>Input mode: window events are reported to the read handler.</summary>
        internal const int ENABLE_WINDOW_INPUT = 0x0008;

        /// <summary>Input mode: mouse events are reported to the read handler.</summary>
        internal const int ENABLE_MOUSE_INPUT = 0x0010;

        /// <summary>Input mode: insert mode replaces characters instead of inserting them.</summary>
        internal const int ENABLE_INSERT_MODE = 0x0020;

        /// <summary>Input mode: text is selected by the mouse.</summary>
        internal const int ENABLE_QUICK_EDIT_MODE = 0x0040;

        /// <summary>Input mode: the extended flags are honoured.</summary>
        internal const int ENABLE_EXTENDED_FLAGS = 0x0080;

        /// <summary>Input mode: the window scrolls to keep the cursor visible after a write.</summary>
        internal const int ENABLE_AUTO_POSITION = 0x0100;

        /// <summary>Output mode: characters are written by the write handler.</summary>
        internal const int ENABLE_PROCESSED_OUTPUT = 0x0001;

        /// <summary>Output mode: text wraps at the end of a line.</summary>
        internal const int ENABLE_WRAP_AT_EOL_OUTPUT = 0x0002;

        /// <summary>Output mode: the console interprets ANSI escape sequences.</summary>
        internal const int ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;

        /// <summary>Output mode: a write does not also move the cursor to the next line.</summary>
        internal const int DISABLE_NEWLINE_AUTO_RETURN = 0x0008;

        /// <summary>Output mode: characters are placed in a grid rather than by cell.</summary>
        internal const int ENABLE_LVB_GRID_WORLDWIDE = 0x0010;

        /// <summary>Reads the mode of a console handle.</summary>
        /// <param name="handle">The console handle to read.</param>
        /// <param name="mode">The mode flags of the handle, when the call succeeds.</param>
        /// <returns>
        /// <see langword="true"/> when the mode was read; <see langword="false"/> when the
        /// handle is not a console, in which case the last error is set.
        /// </returns>
        [LibraryImport(Libs.Kernel32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool GetConsoleMode(IntPtr handle, out int mode);

        /// <summary>Tests whether a handle has a readable console mode.</summary>
        /// <param name="handle">The handle to test.</param>
        /// <returns>
        /// <see langword="true"/> when the handle is a console with a readable mode;
        /// <see langword="false"/> when it is redirected or otherwise not a console.
        /// </returns>
        internal static bool IsGetConsoleModeCallSuccessful(IntPtr handle)
        {
            return GetConsoleMode(handle, out _);
        }

        /// <summary>Writes the mode of a console handle.</summary>
        /// <param name="handle">The console handle to write.</param>
        /// <param name="mode">The mode flags to set.</param>
        /// <returns>
        /// <see langword="true"/> when the mode was written; <see langword="false"/>
        /// when the handle is not a console, in which case the last error is set.
        /// </returns>
        [LibraryImport(Libs.Kernel32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool SetConsoleMode(IntPtr handle, int mode);
    }
}
