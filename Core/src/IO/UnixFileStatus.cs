namespace Neotales.IO
{

    /// <summary>
    /// The status of a file system entry, in the shape the unix <c>stat</c> call
    /// returns it.
    /// </summary>
    /// <remarks>
    /// On unix the values come from the <c>stat</c> call, so a user id, a group id
    /// and an inode number are real. On Windows there is no equivalent, and the
    /// fields that Windows does not expose are <c>-1</c> or a value derived from
    /// <see cref="FileInfo"/>. Test for <c>-1</c> rather than assuming a platform.
    /// </remarks>
    public readonly struct UnixFileStatus
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnixFileStatus"/> struct from
        /// the raw <c>stat</c> layout.
        /// </summary>
        /// <param name="path">The path that was stat'ed.</param>
        /// <param name="status">The raw status the interop call returned.</param>
        internal UnixFileStatus(string path, FFI.Sys.FileStatus status)
        {
            this.Path = path;
            this.Mode = status.Mode;
            this.UserId = (int)status.Uid;
            this.GroupId = (int)status.Gid;
            this.Size = status.Size;
            this.AccessTime = status.ATime;
            this.ModifiedTime = status.MTime;
            this.ChangedTime = status.CTime;
            this.BirthTime = status.BirthTime;
            this.Dev = status.Dev;
            this.Ino = status.Ino;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnixFileStatus"/> struct from
        /// a <see cref="FileInfo"/>, for the platforms with no <c>stat</c>.
        /// </summary>
        /// <param name="path">The path that was inspected.</param>
        /// <param name="info">The file information the platform did give.</param>
        /// <remarks>
        /// The mode is synthesised from the entry kind, the ownership fields are
        /// <c>-1</c> because <see cref="FileInfo"/> has no equivalent, the change
        /// time falls back to the write time and the birth time to the creation
        /// time.
        /// </remarks>
        internal UnixFileStatus(string path, FileInfo info)
        {
            this.Path = path;
            if (info.LinkTarget is not null)
            {
                this.Mode = 0xA000; // S_IFLNK bit for symbolic links
            }
            else if (info.Attributes.HasFlag(FileAttributes.Directory))
            {
                this.Mode = 0x4000; // S_IFDIR bit for directories
            }
            else
            {
                this.Mode = 0x8000; // S_IFREG bit for regular files
            }

            this.UserId = -1; // User ID is not available in FileInfo
            this.GroupId = -1; // Group ID is not available in FileInfo
            this.Size = info.Length;
            this.AccessTime = new DateTimeOffset(info.LastAccessTime).ToUnixTimeSeconds();
            this.ModifiedTime = new DateTimeOffset(info.LastWriteTime).ToUnixTimeSeconds();
            this.ChangedTime = new DateTimeOffset(info.LastWriteTime).ToUnixTimeSeconds(); // Changed time is the last write time
            this.BirthTime = new DateTimeOffset(info.CreationTime).ToUnixTimeSeconds(); // Birth time is the creation time
            this.Dev = -1; // Device ID is not available in FileInfo
            this.Ino = -1; // Inode number is not available in FileInfo
        }

        /// <summary>Gets the path the status was read for.</summary>
        public string Path
        {
            get;
        }

        /// <summary>
        /// Gets the unix mode bits, where the file type is in the high four bits and
        /// the permissions are in the low nine.
        /// </summary>
        public int Mode
        {
            get;
        }

        /// <summary>Gets the owning user id, or <c>-1</c> where the platform does not give one.</summary>
        public int UserId
        {
            get;
        }

        /// <summary>Gets the owning group id, or <c>-1</c> where the platform does not give one.</summary>
        public int GroupId
        {
            get;
        }

        /// <summary>Gets the size in bytes.</summary>
        public long Size
        {
            get;
        }

        /// <summary>Gets the last access time, in seconds since the unix epoch.</summary>
        public long AccessTime
        {
            get;
        }

        /// <summary>Gets the last write time, in seconds since the unix epoch.</summary>
        public long ModifiedTime
        {
            get;
        }

        /// <summary>Gets the last metadata change time, in seconds since the unix epoch.</summary>
        public long ChangedTime
        {
            get;
        }

        /// <summary>Gets the creation time, in seconds since the unix epoch.</summary>
        public long BirthTime
        {
            get;
        }

        /// <summary>Gets the device id, or <c>-1</c> where the platform does not give one.</summary>
        public long Dev
        {
            get;
        }

        /// <summary>Gets the inode number, or <c>-1</c> where the platform does not give one.</summary>
        public long Ino
        {
            get;
        }

        /// <summary>Gets a value indicating whether the entry is a directory.</summary>
        public bool IsDirectory => (this.Mode & 0x4000) != 0; // S_IFDIR bit

        /// <summary>Gets a value indicating whether the entry is a regular file.</summary>
        public bool IsFile => (this.Mode & 0x8000) != 0; // S_IFREG bit

        /// <summary>Gets a value indicating whether the entry is a symbolic link.</summary>
        public bool IsSymbolicLink => (this.Mode & 0xA000) != 0; // S_IFLNK bit

        /// <summary>
        /// Gets the user name associated with this file's user ID.
        /// </summary>
        /// <returns>The user name, or <see langword="null"/> if unavailable.</returns>
        /// <remarks>
        /// <example>
        /// <code lang="csharp">
        /// var status = File.Stat("/etc/passwd");
        /// var userName = status.GetUserName();
        /// </code>
        /// </example>
        /// </remarks>
        public string? GetUserName()
        {
            if (this.UserId < 0)
                return null;

            var uid = (uint)this.UserId;
            return FFI.Sys.GetUserName(uid);
        }

        /// <summary>
        /// Gets the group name associated with this file's group ID.
        /// </summary>
        /// <returns>The group name, or <see langword="null"/> if unavailable.</returns>
        /// <remarks>
        /// <example>
        /// <code lang="csharp">
        /// var status = File.Stat("/etc/passwd");
        /// var groupName = status.GetGroupName();
        /// </code>
        /// </example>
        /// </remarks>
        public string? GetGroupName()
        {
            if (this.GroupId < 0)
                return null;

            var gid = (uint)this.GroupId;
            return FFI.Libc.GetGroupName(gid);
        }
    }
}
