namespace System
{
    internal static class StringExtensions
    {
        public static long? ToInt64Nullable(this string Expr)
        {
            long? result = null;

            if (string.IsNullOrWhiteSpace(Expr))
                return result;

            Expr = Expr.Trim();

            long newInt64;

            if (long.TryParse(Expr, out newInt64))
                result = newInt64;

            return result;
        }

        public static long ToInt64(this string Expr, long DefaultValue = 0)
        {
            return ToInt64Nullable(Expr) ?? DefaultValue;
        }
    }
}