using System.Collections.Generic;

namespace Docdown.Model
{
    public static class BibliographyParser
    {
        public static IEnumerable<BibliographyEntry> Parse(IWorkspaceItem item, string text)
        {
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '@')
                {
                    var entry = ParseEntry(item, text, ref i);
                    if (entry != null)
                    {
                        yield return entry;
                    }
                }
            }
        }

        private static BibliographyEntry ParseEntry(IWorkspaceItem item, string text, ref int index)
        {
            int start = ++index;
            string type = null;
            string key = null;
            var fields = new Dictionary<string, string>();
            for (int i = start; i < text.Length; i++)
            {
                char c = text[i];
                if (!char.IsLetterOrDigit(c) && c != ' ')
                {
                    if (c != '{')
                    {
                        return null;
                    }
                    type = text.Substring(start, i - start).Trim();
                    key = ParseKey(text, ref i);
                    if (key == null)
                    {
                        return null;
                    }
                    fields = ParseFields(text, ref i);
                    index = i;
                    break;
                }
            }
            return new BibliographyEntry(item, key, type, fields);
        }

        private static string ParseKey(string text, ref int index)
        {
            index++;
            for (int i = index; i < text.Length; i++)
            {
                if (text[i] == ',' || text[i] == '}')
                {
                    var key = text.Substring(index, i - index);
                    index = i;
                    return key;
                }
            }
            return null;
        }

        private static Dictionary<string, string> ParseFields(string text, ref int index)
        {
            if (text[index++] != ',')
            {
                return null;
            }

            var dic = new Dictionary<string, string>();
            for (int i = index; i < text.Length; i++)
            {
                if (char.IsLetterOrDigit(text[i]))
                {
                    string name = ParseFieldName(text, ref i);
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        goto END;
                    }
                    var value = ParseFieldValue(text, 0, ref i);
                    if (value == null)
                    {
                        goto END;
                    }
                    dic[name] = value;
                }
                else if (text[i] == '}')
                {
                    goto END;
                }
                continue;

            END:
                index = i;
                return dic;
            }
            return null;
        }

        private static string ParseFieldName(string text, ref int index)
        {
            for (int i = index; i < text.Length; i++)
            {
                if (!char.IsLetterOrDigit(text[i]))
                {
                    var name = text.Substring(index, i - index);
                    index = i;
                    return name;
                }
            }
            return null;
        }

        private static string ParseFieldValue(string text, int stack, ref int index)
        {
            int start = index;
            for (int i = start; i < text.Length; i++)
            {
                if (text[i] == '{')
                {
                    start = i;
                    break;
                }
            }
            if (start == index)
            {
                return null;
            }

            for (int i = start; i < text.Length; i++)
            {
                if (text[i] == '{')
                {
                    stack++;
                }
                else if (text[i] == '}' && --stack == 0)
                {
                    index = i;
                    return text.Substring(start, i - start).TrimStart('{').TrimEnd('}');
                }
            }
            return null;
        }

        private static bool ValidKeyCharacter(char c)
        {
            return c != ',' && c != '}' && c != ' ';
        }
    }
}
