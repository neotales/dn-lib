using Neotales.IO.Extras;

namespace Neotales.IO.Tests
{

    /// <summary>
    /// The <c>stat(2)</c> interop behind <see cref="UnixFileStatus.Ino"/>, asserted
    /// against the public surface rather than trusted.
    /// </summary>
    /// <remarks>
    /// Status is read through libc and mapped field by field. These tests assert
    /// the public inode, device, and size, not the layout of a native overlay.
    /// </remarks>
    public static class FileStatusInodeTests
    {

        /// <summary>
        /// A file on this machine has a non-zero inode, as the platform reports it.
        /// </summary>
        [Fact]
        public static void Stat_Reports_A_Non_Zero_Inode_On_Unix()
        {
            if (OperatingSystem.IsWindows())
            {
                Assert.Skip("Unix inode numbers are not available on Windows.");
            }

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
        [Fact]
        public static void Two_Different_Files_Have_Different_Inodes()
        {
            if (OperatingSystem.IsWindows())
            {
                Assert.Skip("Unix inode numbers are not available on Windows.");
            }

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
        /// The device id and size come from the same status call as the inode.
        /// </summary>
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
