using System;
using System.Text;

namespace Docdown.Util
{
    public static class TextUtility
    {
        public static string Increase(char c)
        {
            if ((c >= 'z' && c < 'A') || c < 'a' || c >= 'Z')
            {
                return null;
            }
            var next = (char)(c + 1);
            return next.ToString();
        }

        public static int GetLineNumber(string text, int index)
        {
            int lines = 0;
            for (int i = 0; i < index; i++)
            {
                if (text[i] == '\n')
                {
                    lines++;
                }
            }
            return lines;
        }

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

        public static bool IsBlank(char value)
        {
            switch (value)
            {
                case ' ':
                case '\n':
                case '\r':
                case '\t':
                    return true;
            }
            return false;
        }
    }
}
