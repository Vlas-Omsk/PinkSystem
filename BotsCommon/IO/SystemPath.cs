namespace BotsCommon.IO
{
    public enum SystemPathType
    {
        Absolute,
        Relative,
    }

    public readonly struct SystemPath
    {
        public SystemPath(string path)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            Type = System.IO.Path.IsPathRooted(Path) ?
                SystemPathType.Absolute :
                SystemPathType.Relative;
        }

        public SystemPath(params string[] paths) :
            this(System.IO.Path.Combine(paths))
        {
        }

        public string Path { get; }
        public SystemPathType Type { get; }

        public SystemPath GetAbsolute()
        {
            return GetAbsoluteToString();
        }

        public SystemPath GetAbsoluteTo(SystemPath basePath)
        {
            return GetAbsoluteToString(basePath.GetAbsoluteToString());
        }

        private string GetAbsoluteToString(string basePath = null)
        {
            basePath ??= Directory.GetCurrentDirectory();

            return Type switch
            {
                SystemPathType.Absolute => Path,
                SystemPathType.Relative => System.IO.Path.GetFullPath(Path, basePath),
                _ => throw new InvalidOperationException(),
            };
        }

        public SystemPath GetRelative()
        {
            return GetRelativeTo(Directory.GetCurrentDirectory());
        }

        public SystemPath GetRelativeTo(SystemPath relativeTo)
        {
            return new SystemPath(
                System.IO.Path.GetRelativePath(relativeTo.GetAbsoluteToString(), GetAbsoluteToString())
            );
        }

        public string GetFileNameWithoutExtension()
        {
            return System.IO.Path.GetFileNameWithoutExtension(Path);
        }

        public string GetExtension()
        {
            return System.IO.Path.GetExtension(Path);
        }

        public override int GetHashCode()
        {
            return GetAbsoluteToString().GetHashCode();
        }

        public bool Equals(string value)
        {
            return GetAbsoluteToString() == System.IO.Path.GetFullPath(value);
        }

        public bool Equals(SystemPath value)
        {
            return Equals(value.Path);
        }

        public override bool Equals(object obj)
        {
            if (obj is string @string)
                return Equals(@string);
            if (obj is SystemPath @path)
                return Equals(@path);

            return base.Equals(obj);
        }

        public override string ToString()
        {
            return Path;
        }

        public static SystemPath operator +(SystemPath left, string right)
        {
            return new SystemPath(left.GetAbsoluteToString(), right);
        }

        public static bool operator ==(SystemPath left, SystemPath right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SystemPath left, SystemPath right)
        {
            return !(left == right);
        }

        public static implicit operator string(SystemPath path)
        {
            return path.Path;
        }

        public static implicit operator SystemPath(string path)
        {
            return new SystemPath(path);
        }
    }
}
