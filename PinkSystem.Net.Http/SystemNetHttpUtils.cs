﻿using PinkSystem.IO.Content;
using PinkSystem.Net.Sockets;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace PinkSystem.Net.Http
{
    public sealed class SystemNetHttpUtils
    {
        public static Func<SocketsHttpConnectionContext, CancellationToken, ValueTask<Stream>> CreateConnectCallback(ISocketsProvider socketsProvider)
        {
            return async (context, cancellationToken) =>
            {
                var socket = await socketsProvider.Create(SocketType.Stream, ProtocolType.Tcp, cancellationToken);

                if (!OperatingSystem.IsLinux())
                    socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

                try
                {
                    await socket.ConnectAsync(context.DnsEndPoint, cancellationToken).ConfigureAwait(false);

                    return socket.GetStream();
                }
                catch
                {
                    socket.Dispose();
                    throw;
                }
            };
        }

        public static HttpRequestMessage CreateNetRequestFromRequest(HttpRequest request)
        {
            var requestMessage = new HttpRequestMessage()
            {
                Method = new HttpMethod(request.Method),
                RequestUri = request.Uri,
                Version = request.Version ?? HttpVersion.Version11,
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
                {
                    if (requestMessage.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value) == false)
                        throw new Exception();
                }
                else
                {
                    if (requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value) == false)
                        throw new Exception();
                }
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

            using var contentStream = await responseMessage.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

            ReadOnlyMemory<byte> contentBytes;

            if (contentStream is MemoryStream memoryStream)
            {
                contentBytes = memoryStream.ToReadOnlyMemory();
            }
            else
            {
                using var memoryStream2 = responseMessage.Content.Headers.ContentLength.HasValue ?
                    new MemoryStream((int)responseMessage.Content.Headers.ContentLength) :
                    new MemoryStream();

                await contentStream.CopyToAsync(memoryStream2, cancellationToken);

                contentBytes = memoryStream2.ToReadOnlyMemory();
            }

            return new HttpResponse()
            {
                Uri = responseMessage.RequestMessage?.RequestUri!,
                StatusCode = responseMessage.StatusCode,
                ReasonPhrase = responseMessage.ReasonPhrase,
                Headers = headers,
                Content = new ByteArrayContentReader(
                    contentBytes,
                    responseMessage.Content.Headers.ContentType?.MediaType ?? "application/octet-stream"
                )
            };
        }
    }
}
