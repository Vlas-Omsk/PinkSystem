using System.IO;

namespace PinkSystem.IO.Content
{
    public interface IContentReader
    {
        long? Length { get; }
        string MimeType { get; }

        Stream CreateStream();
    }
}
