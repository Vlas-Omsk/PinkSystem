using System.IO;

namespace BotsCommon.IO.Content
{
    public interface IContentReader
    {
        long? Length { get; }
        string MimeType { get; }

        Stream CreateStream();
    }
}
