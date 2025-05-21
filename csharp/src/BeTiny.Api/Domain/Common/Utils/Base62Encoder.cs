using System.Text;

namespace BeTiny.Api.Domain.Common.Utils
{
    public static class Base62Encoder
    {
        private static readonly string _base62Chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        public static string Encode(long value)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    "Only positive numbers are supported"
                );
            }

            var result = new StringBuilder();
            do
            {
                result.Insert(0, _base62Chars[(int)(value % 62)]);
                value /= 62;
            }
            while (value > 0);

            return result.ToString();
        }
    }
}
