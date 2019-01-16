namespace System
{
    internal static class LongExtensions
    {
        public static DateTime ToUnixEpochDate(this long unixTime)
        {
            var result = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(unixTime);

            return result;
        }
    }
}