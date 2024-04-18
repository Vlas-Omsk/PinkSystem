using BotsCommon.IO.Content;
using BotsCommon.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace BotsCommon.Net.Http.Handlers
{
    public sealed class SystemWebHttpRequestHandler : IHttpRequestHandler
    {
        private static readonly Dictionary<string, PropertyInfo> _optionProperties = new(StringComparer.OrdinalIgnoreCase);

        public SystemWebHttpRequestHandler(HttpRequestHandlerOptions options)
        {
            Options = options;
        }

        static SystemWebHttpRequestHandler()
        {
            Type type = typeof(HttpWebRequest);

            foreach (PropertyInfo property in type.GetProperties())
                _optionProperties[property.Name] = property;
        }

        public HttpRequestHandlerOptions Options { get; }

        public async Task<HttpResponse> SendAsync(HttpRequest request, CancellationToken cancellationToken)
        {
#pragma warning disable SYSLIB0014 // Type or member is obsolete
            var webRequest = WebRequest.CreateHttp(request.Uri);
#pragma warning restore SYSLIB0014 // Type or member is obsolete

            if (Options.Proxy != null)
                webRequest.Proxy = Options.Proxy.ToWebProxy();

            webRequest.ProtocolVersion = HttpVersion.Version11;
            webRequest.Method = request.Method;

            foreach (var header in request.Headers)
            {
                foreach (var value in header.Value)
                {
                    var optionName = header.Key.Replace("-", "");
                    
                    if (_optionProperties.TryGetValue(optionName, out var propertyInfo))
                        propertyInfo.SetValue(webRequest, TypeConverter.ChangeType(value, propertyInfo.PropertyType));
                    else
                        webRequest.Headers.Add(header.Key, value);
                }
            }

            if (request.Content != null)
            {
                using (var webRequestStream = await webRequest.GetRequestStreamAsync().ConfigureAwait(false))
                using (var requestStream = request.Content.CreateStream())
                    requestStream.CopyTo(webRequestStream);
            }

            using var webResponse = (HttpWebResponse)await webRequest.GetResponseAsync().ConfigureAwait(false);

            ReadOnlyMemory<byte> bytes;

            using (var webResponseStream = webResponse.GetResponseStream())
            using (var memoryStream = new MemoryStream(256))
            {
                webResponseStream.CopyTo(memoryStream);

                bytes = memoryStream.ToReadOnlyMemory();
            }

            var headers = new HttpHeaders();

            foreach (var key in webResponse.Headers.AllKeys)
                headers.Add(key, webResponse.Headers.Get(key) ?? throw new Exception($"Response header value of '{key}' cannot be null"));

            return new HttpResponse(
                webResponse.ResponseUri,
                webResponse.StatusCode,
                null,
                headers,
                new ByteArrayContentReader(
                    bytes,
                    webResponse.ContentType
                )
            );
        }

        public void Dispose()
        {
        }
    }
}
