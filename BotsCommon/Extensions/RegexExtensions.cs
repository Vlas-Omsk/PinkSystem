using System.Text.RegularExpressions;

namespace BotsCommon
{
    public static class RegexExtensions
    {
        public static Group ThrowIfNotSuccuess(this Group self)
        {
            if (!self.Success)
                throw new Exception($"Group {self.Index} ({self.Name}) not success");

            return self;
        }
    }
}
