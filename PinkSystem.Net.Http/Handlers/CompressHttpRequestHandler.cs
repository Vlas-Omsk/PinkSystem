﻿using PinkSystem.IO.Content;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PinkSystem.Net.Http.Handlers
{
    public sealed class CompressHttpRequestHandler : ExtensionHttpRequestHandler
    {
        private sealed class DecompressContentReader : IContentReader
        {
            private readonly IContentReader _contentReader;
            private readonly string _contentEncoding;

            public DecompressContentReader(IContentReader contentReader, string contentEncoding)
            {
                _contentReader = contentReader;
                _contentEncoding = contentEncoding;
            }

            public long? Length => _contentReader.Length;
            public string MimeType => _contentReader.MimeType;

            public Stream CreateStream()
            {
                switch (_contentEncoding)
                {
                    case "br":
                        return new BrotliStream(_contentReader.CreateStream(), CompressionMode.Decompress);
                    case "gzip":
                        return new GZipStream(_contentReader.CreateStream(), CompressionMode.Decompress);
                    case "deflate":
                        return new DeflateStream(_contentReader.CreateStream(), CompressionMode.Decompress);
                    default:
                        return _contentReader.CreateStream();
                }
            }
        }

        public CompressHttpRequestHandler(IHttpRequestHandler handler) : base(handler)
        {
        }

        public override async Task<HttpResponse> SendAsync(HttpRequest request, CancellationToken cancellationToken)
        {
            request.Headers.Replace("Accept-Encoding", "gzip, deflate, br");

            var response = await Handler.SendAsync(request, cancellationToken).ConfigureAwait(false);

            response = new HttpResponse(
                response.Uri,
                response.StatusCode,
                response.ReasonPhrase,
                response.Headers,
                new DecompressContentReader(
                    response.Content,
                    response.Headers.TryGetValues("Content-Encoding", out var values) ?
                        values.Single() :
                        string.Empty
                )
            );

            return response;
        }
    }
}
