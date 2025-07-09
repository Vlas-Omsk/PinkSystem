using PinkSystem.Net;

namespace PinkSystem.IO.Content
{
    public sealed class FormUrlEncodedContentReader : StringContentReader
    {
        public FormUrlEncodedContentReader(IReadOnlyQueryData data) : base(data.ToString(), "application/x-www-form-urlencoded")
        {
        }
    }
}
