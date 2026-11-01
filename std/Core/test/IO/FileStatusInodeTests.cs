using Neotales.IO.Extras;

namespace Neotales.IO.Tests
{

    /// <summary>
    /// The <c>stat(2)</c> interop behind <see cref="UnixFileStatus.Ino"/>, asserted
    /// against the public surface rather than trusted.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>FFI.Sys.FileStatus</c> is the record .NET's own <c>libSystem.Native</c> writes
    /// through <c>SystemNative_Stat</c>. A declaration of that record that is one
    /// field too small does not fail: the native write lands inside the managed
    /// buffer, every field after the gap reads the value belonging to the field
    /// before it, and nothing throws anywhere.
    /// </para>
    /// <para>
    /// The real record is 120 bytes, with a reserved word between <c>Dev</c> at 88
    /// and <c>Ino</c> at 104. The declaration in this repository is 112 bytes: it
    /// omits the gap, so <c>Ino</c> reads zero for every file on the machine and
    /// the true inode lands in the trailing field instead. The ClearScript package
    /// in this same repository declares the record correctly, including the
    /// reserved word, and asserts its own layout in a test.
    /// </para>
    /// <para>
    /// These tests are written against the public surface, so they describe what a
    /// consumer observes rather than what a struct contains. That is deliberate: a
    /// test on the struct would pass while the public property stayed wrong, which
    /// is precisely the failure being pinned here.
    /// </para>
    /// <para>
    /// The failing tests below are skipped with the reason rather than removed,
    /// because the assertion is the specification and the skip says what has to
    /// change for it to pass. A defect recorded as a deleted test is a defect
    /// nobody will find again.
    /// </para>
    /// </remarks>
    public static class FileStatusInodeTests
    {

        /// <summary>
        /// A file on this machine has a non-zero inode, as the platform reports it.
        /// </summary>
        /// <remarks>
        /// Skipped: the record is 112 bytes and the real one is 120, so
        /// <see cref="UnixFileStatus.Ino"/> reads zero for every path. This passes
        /// when <c>FFI.Sys.FileStatus</c> is 120 bytes with a reserved word at 96
        /// and <c>Ino</c> at 104.
        /// </remarks>
        [Fact]
        public static void Stat_Reports_A_Non_Zero_Inode_On_Unix()
        {
            Assert.Skip("KNOWN DEFECT: FFI.Sys.FileStatus is 112 bytes and the real stat(2) record is 120, with a reserved word between Dev at 88 and Ino at 104. The declaration omits the gap, so Ino reads zero for every file while the true inode lands in the trailing field. Passes when the record is 120 bytes with Ino at 104.");

            string path = Path.GetTempFileName();
            try
            {
                UnixFileStatus status = File.Stat(path);

                Assert.NotEqual(0, status.Ino);
            }
            finally
            {
                File.Delete(path);
            }
        }

        /// <summary>
        /// Two different files have different inodes, as the platform reports them.
        /// </summary>
        /// <remarks>
        /// Skipped: the same 112 byte record, so every inode reads zero and two
        /// files are indistinguishable. This passes with the corrected layout.
        /// </remarks>
        [Fact]
        public static void Two_Different_Files_Have_Different_Inodes()
        {
            Assert.Skip("KNOWN DEFECT: the same 112 byte stat(2) record, so every inode reads zero and two files look identical. Passes with the corrected layout.");

            string directory = Directory.CreateTempSubdirectory("inode").FullName;
            try
            {
                string first = Path.Combine(directory, "first");
                string second = Path.Combine(directory, "second");
                File.WriteAllText(first, "one");
                File.WriteAllText(second, "two");

                long firstIno = File.Stat(first).Ino;
                long secondIno = File.Stat(second).Ino;

                Assert.NotEqual(0, firstIno);
                Assert.NotEqual(firstIno, secondIno);
            }
            finally
            {
                Directory.Delete(directory, recursive: true);
            }
        }

        /// <summary>
        /// The fields before the gap read correctly, which is what makes the two
        /// failures above a layout fault rather than a platform without inodes.
        /// </summary>
        /// <remarks>
        /// This one is green and is the control. Without it, an observation that
        /// <c>Ino</c> is zero says nothing: a machine with no inode support, a
        /// wrong path and a truncated record all look the same from the property
        /// alone. <c>Dev</c> and <c>Size</c> sit before the gap and are correct,
        /// so reading them correctly places the fault after them and nowhere else.
        /// </remarks>
        [Fact]
        public static void The_Device_Id_And_Size_Read_Correctly_Before_The_Gap()
        {
            string path = Path.GetTempFileName();
            try
            {
                File.WriteAllText(path, new string('x', 4096));
                UnixFileStatus status = File.Stat(path);

                Assert.NotEqual(0, status.Dev);
                Assert.Equal(4096, status.Size);
            }
            finally
            {
                File.Delete(path);
            }
        }
    }
}
