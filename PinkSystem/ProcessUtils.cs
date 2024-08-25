using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace PinkSystem
{
    public static class ProcessUtils
    {
        public static async Task<string> GetProcessOutput(string fileName, string arguments)
        {
            var output = new StringBuilder();

            void OutputHandler(object sender, DataReceivedEventArgs e)
            {
                output.AppendLine(e.Data);
            }

            using var process = new Process();

            process.StartInfo.FileName = fileName;
            process.StartInfo.Arguments = arguments;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;

            process.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
            process.ErrorDataReceived += new DataReceivedEventHandler(OutputHandler);

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync();

            return output.ToString();
        }
    }
}
