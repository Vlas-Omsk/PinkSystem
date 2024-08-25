using System.IO;

namespace PinkSystem
{
    public static class StreamReaderExtensions
    {
        public static void SetPosition(this StreamReader self, long position)
        {
            self.BaseStream.Position = position;
            self.DiscardBufferedData();
        }

        public static int CountLines(this StreamReader self)
        {
            var position = self.BaseStream.Position;

            self.SetPosition(0);

            var count = 0;

            while (self.ReadLine() != null)
                count++;

            self.SetPosition(position);

            return count;
        }
    }
}
