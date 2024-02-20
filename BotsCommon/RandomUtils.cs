namespace BotsCommon
{
    public static class RandomUtils
    {
        private static readonly char[] _chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();

        public static string GenerateRandomString(int length)
        {
            var stringChars = new char[length];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
                stringChars[i] = _chars[random.Next(_chars.Length)];

            return new string(stringChars);
        }
    }
}
