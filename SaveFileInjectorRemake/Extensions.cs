using System.Linq;
using System.Text.RegularExpressions;

namespace SaveFileInjectorRemake
{
    public static class Extensions
    {
        public static bool IsBase64String(this string input)
        {
            input = input.Trim();
            return (input.Length % 4 == 0) && Regex.IsMatch(input, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None);

        }

        public static bool IsDeadByDaylightCryptoString(this string input)
        {
            string cropped = new string(input.Take(8).ToArray());
            if (cropped == "DbdDAgAC")
                return true;
            else return false;

        }
    }
}
