using System;

namespace BotsCommon.Net
{
    public sealed class UriBuilder
    {
        public UriBuilder(string uriString) : this(new Uri(uriString))
        {
        }

        public UriBuilder(Uri uri)
        {
            Uri = uri;
        }

        public Uri Uri { get; private set; }

        public UriBuilder AddRelativeUri(Uri relativeUri)
        {
            Uri = new Uri(Uri, relativeUri);

            return this;
        }

        public UriBuilder AddRelativeUri(string relativeUri)
        {
            Uri = new Uri(Uri, relativeUri);

            return this;
        }

        public UriBuilder AddQuery(QueryData query)
        {
            Uri = new Uri($"{Uri}?{query}");

            return this;
        }

        public UriBuilder AddHash(string hash)
        {
            Uri = new Uri($"{Uri}#{hash}");

            return this;
        }
    }
}
