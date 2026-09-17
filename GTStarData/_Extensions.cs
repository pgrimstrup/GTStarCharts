using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GTStarData
{
    public static class _Extensions
    {
        public static string Subcode(this string text, int position)
        {
            if (String.IsNullOrEmpty(text)) return null;
            
            var chars = text.ToCharArray().Where(ch => Char.IsLetterOrDigit(ch)).ToArray();
            if (chars.Length <= position) return null;
            return chars[position].ToString();
        }

        const string alphahex = "0123456789ABCDEFGHJKLMNPQRSTUVWXYZ";

        public static int ToInt(this string text)
        {
            if (String.IsNullOrEmpty(text)) return -1;
            if (text.Length > 1) return -1;
            return alphahex.IndexOf(text[0]);
        }

        public static int D6(this Random r, int count = 1)
        {
            int result = 0;
            for (int i = 0; i < count; i++)
                result += r.Next(1, 7);
            return result;
        }

        public static int D66(this Random r, int mod1 = 0, int mod2 = 0)
        {
            return (r.Next(1, 7) + mod1).Min(0).Max(9) * 10 + (r.Next(1, 7) + mod2).Min(0).Max(9);
        }

        public static int Min(this int value, int min)
        {
            if (value < min) return min;
            return value;
        }

        public static int Max(this int value, int max)
        {
            if (value > max) return max;
            return value;
        }
    }
}
