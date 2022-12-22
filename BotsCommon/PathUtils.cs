using System;

namespace BotsCommon
{
    public static class PathUtils
    {
        public static string ReplaceFileNameInvalidChars(string fileName, char ch)
        {
            foreach (var invalid in Path.GetInvalidFileNameChars())
                fileName = fileName.Replace(invalid, ch);

            return fileName;
        }
    }
}
