using System;
using System.IO;
using System.Linq;

namespace PinkSystem
{
    public static class PathUtils
    {
        private static readonly string[] _invalidFileNameChars = Path.GetInvalidFileNameChars().Select(x => x.ToString()).ToArray();

        public static string ReplaceFileNameInvalidChars(string fileName, string ch)
        {
            foreach (var invalid in _invalidFileNameChars)
                fileName = fileName.Replace(invalid, ch);

            return fileName;
        }

        public static string ReplaceFileNameInvalidChars(string fileName, char ch)
        {
            foreach (var invalid in Path.GetInvalidFileNameChars())
                fileName = fileName.Replace(invalid, ch);

            return fileName;
        }

        public static bool IsExecutableExists(string path)
        {
            if (OperatingSystem.IsWindows() && !path.EndsWith(".exe"))
                path += ".exe";

            if (File.Exists(path))
                return true;

            string[] pathVariable;

            if (OperatingSystem.IsWindows())
                pathVariable = (Environment.GetEnvironmentVariable("PATH") ?? string.Empty).Split(';');
            else if (OperatingSystem.IsLinux())
                pathVariable = (Environment.GetEnvironmentVariable("PATH") ?? string.Empty).Split(':');
            else
                throw new PlatformNotSupportedException();

            return pathVariable.Any(x => File.Exists(Path.Combine(x, path)));
        }
    }
}
