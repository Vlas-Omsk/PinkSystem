using BotsCommon.IO.Content;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BotsCommon.Net.Http
{
    internal sealed class SystemNetHttpUtils
    {
        public static HttpRequestMessage CreateNetRequestFromRequest(HttpRequest request)
        {
            var requestMessage = new HttpRequestMessage()
            {
                Method = new HttpMethod(request.Method),
                RequestUri = request.Uri,
                Version = HttpVersion.Version20,
                VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
            };

            if (request.Content != null)
            {
                requestMessage.Content = new StreamContent(request.Content.CreateStream());

                requestMessage.Content.Headers.Clear();
            }

            foreach (var header in request.Headers)
            {
                if (header.Key.StartsWith("Content-", StringComparison.OrdinalIgnoreCase))
                    requestMessage.Content?.Headers.Add(header.Key, header.Value);
                else
                    requestMessage.Headers.Add(header.Key, header.Value);
            }

            return requestMessage;
        }

        public static async Task<HttpResponse> CreateResponseFromNetResponse(HttpResponseMessage responseMessage, CancellationToken cancellationToken)
        {
            var headers = new HttpHeaders();

            foreach (var header in responseMessage.Headers)
                headers.Add(header.Key, header.Value);

            foreach (var header in responseMessage.Content.Headers)
                headers.Add(header.Key, header.Value);

            var contentBytes = await responseMessage.Content.ReadAsByteArrayAsync(cancellationToken);

            return new HttpResponse(
                responseMessage.RequestMessage?.RequestUri!,
                responseMessage.StatusCode,
                responseMessage.ReasonPhrase,
                headers,
                new ByteArrayContentReader(
                    contentBytes,
                    responseMessage.Content.Headers.ContentType?.MediaType ?? "application/octet-stream"
                )
            );
        }
    }
}
