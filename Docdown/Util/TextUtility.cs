using System;
using System.Text;

namespace Docdown.Util
{
    public static class TextUtility
    {
        public static string InsertOnUpper(string value, string insert = " ")
        {
            return InsertOn(value, insert, c => char.IsUpper(c));
        }

        public static string InsertOn(string value, string insert, Func<char, bool> determinator)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            StringBuilder sb = new StringBuilder(value[0]);
            for (int i = 1; i < value.Length; i++)
            {
                char c = value[i];

                if (determinator(c))
                {
                    sb.Append(insert);
                }
                sb.Append(c);
            }
            return sb.ToString();
        }
    }
}
