using System;

namespace BotsCommon
{
    public sealed class ConsoleProgress : IProgress<string>
    {
        private readonly string _prefix;

        public ConsoleProgress() : this(GetDefaultPrefix())
        {
        }

        public ConsoleProgress(string prefix)
        {
            _prefix = prefix;

            Report(null);
        }

        public void Report(string current)
        {
            var title = _prefix;

            if (current != null)
                title += " | " + current;

            Console.Title = title;
        }

        private static string GetDefaultPrefix()
        {
            var assemblyName = System.Reflection.Assembly.GetEntryAssembly().GetName();

            return assemblyName.Name + " v" + assemblyName.Version;
        }
    }
}
