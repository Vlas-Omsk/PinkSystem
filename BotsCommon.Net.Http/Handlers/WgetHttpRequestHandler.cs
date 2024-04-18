using BotsCommon.IO.Content;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BotsCommon.Net.Http.Handlers
{
    public sealed class WgetHttpRequestHandler : IHttpRequestHandler
    {
        private readonly int _retryAmount;
        private readonly ILogger<WgetHttpRequestHandler> _logger;

        public WgetHttpRequestHandler(
            HttpRequestHandlerOptions options,
            int retryAmount,
            ILogger<WgetHttpRequestHandler> logger
        )
        {
            Options = options;
            _retryAmount = retryAmount;
            _logger = logger;
        }

        public HttpRequestHandlerOptions Options { get; }

        public async Task<HttpResponse> SendAsync(HttpRequest request, CancellationToken cancellationToken)
        {
            using var process = new Process();

            process.StartInfo.FileName = "wget";

            process.StartInfo.ArgumentList.Add("--no-verbose");
            process.StartInfo.ArgumentList.Add("--server-response");
            process.StartInfo.ArgumentList.Add("--output-document=-");
            process.StartInfo.ArgumentList.Add("--max-redirect=0");
            process.StartInfo.ArgumentList.Add("--content-on-error");
            process.StartInfo.ArgumentList.Add("--ignore-length");
            process.StartInfo.ArgumentList.Add("--retry-on-host-error");
            process.StartInfo.ArgumentList.Add("--retry-connrefused");
            process.StartInfo.ArgumentList.Add($"--tries={_retryAmount}");
            process.StartInfo.ArgumentList.Add($"--method={request.Method}");

            string? tempFileName = null;

            try
            {
                if (request.Content != null)
                {
                    tempFileName = Path.GetTempFileName();

                    using (var stream = request.Content.CreateStream())
                    using (var fileStream = File.OpenWrite(tempFileName))
                        stream.CopyTo(fileStream);

                    process.StartInfo.ArgumentList.Add($"--body-file={tempFileName}");
                }

                if (Options.Proxy != null)
                {
                    process.StartInfo.ArgumentList.Add("--execute");
                    process.StartInfo.ArgumentList.Add("use_proxy=yes");
                    process.StartInfo.ArgumentList.Add("--execute");
                    process.StartInfo.ArgumentList.Add($"http_proxy={Options.Proxy.GetUri(useCredentials: true)}");
                }

                foreach (var header in request.Headers)
                    foreach (var value in header.Value)
                        process.StartInfo.ArgumentList.Add($"--header={header.Key}: {value}");

                process.StartInfo.ArgumentList.Add(request.Uri.ToString());

                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.EnableRaisingEvents = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.RedirectStandardOutput = true;

                process.Start();

                HttpStatusCode? statusCode = null;
                string? reasonPhrase = null;
                var errorsLog = new StringBuilder();
                var logMessageBuffer = new StringBuilder();
                var headers = new HttpHeaders();

                string? line;
                var firstLine = true;

                ReadOnlyMemory<byte>? contentBytes = null;

                await Task.WhenAll(
                    Task.Run(() =>
                    {
                        while ((line = process.StandardError.ReadLine()) != null)
                        {
                            if (line[..2] == "  ")
                            {
                                var trimmedLine = line[2..];

                                if (firstLine)
                                {
                                    var split = trimmedLine.Split(' ');

                                    statusCode = (HttpStatusCode)int.Parse(split[1]);
                                    reasonPhrase = string.Join(' ', split[2..]);

                                    firstLine = false;
                                }
                                else
                                {
                                    var delimiterIndex = trimmedLine.IndexOf(':');

                                    var key = trimmedLine[..delimiterIndex];
                                    var value = trimmedLine[(delimiterIndex + 1)..];

                                    headers.Add(key, value.TrimStart());
                                }
                            }
                            else
                            {
                                LogWgetOutput(logMessageBuffer, line);

                                errorsLog.AppendLine(line);
                            }
                        }
                    }),
                    Task.Run(() =>
                    {
                        using (var memoryStream = new MemoryStream(0))
                        {
                            process.StandardOutput.BaseStream.CopyTo(memoryStream);

                            contentBytes = memoryStream.ToReadOnlyMemory();
                        }
                    }),
                    process.WaitForExitAsync(cancellationToken)
                ).ConfigureAwait(false);

                if (!statusCode.HasValue)
                    throw new Exception(errorsLog.ToString());

                return new HttpResponse(
                    request.Uri,
                    statusCode.Value,
                    reasonPhrase,
                    headers,
                    new ByteArrayContentReader(
                        contentBytes!.Value,
                        headers.TryGetValues("Content-Type", out var values) ? values.Single() : "application/octet-stream"
                    )
                );
            }
            finally
            {
                if (tempFileName != null)
                    File.Delete(tempFileName);
            }
        }

        private void LogWgetOutput(StringBuilder logMessageBuffer, string line)
        {
            if (!_logger.IsEnabled(LogLevel.Information))
                return;

            if (line.Contains("-> \"-\"") ||
                line.Contains("0 redirections exceeded."))
                return;

            if (logMessageBuffer.Length > 0)
                logMessageBuffer.AppendLine();

            logMessageBuffer.Append(line);

            if (line.EndsWith(':'))
                return;

            var message = logMessageBuffer.ToString();

            _logger.Log(
                message.Contains("err", StringComparison.OrdinalIgnoreCase) ||
                message.Contains("fail", StringComparison.OrdinalIgnoreCase) ||
                message.Contains("warn", StringComparison.OrdinalIgnoreCase) ?
                    LogLevel.Warning :
                    LogLevel.Information,
                message
            );

            logMessageBuffer.Clear();
        }

        public void Dispose()
        {
        }
    }
}
