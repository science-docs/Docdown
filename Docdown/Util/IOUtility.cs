using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Docdown.Util
{
    public static class IOUtility
    {
        private static readonly Regex InvalidFileNameRegex = new Regex("[" + new string(Path.GetInvalidFileNameChars()) + "]", RegexOptions.Compiled);
        private static readonly MD5 md5 = MD5.Create();

        public static string GetTempFile(string name = null)
        {
            string temp = Path.GetTempPath();
            const string docdown = "Docdown";
            string randomFile = (name ?? Guid.NewGuid().ToString()) + ".tmp";
            string file = Path.Combine(temp, docdown, randomFile);
            string dir = Path.Combine(temp, docdown);
            Directory.CreateDirectory(dir);
            return file;
        }

        public static string GetHashFile(string valueToHash)
        {
            var bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(valueToHash));
            return GetTempFile(ByteArrayToString(bytes));
        }

        public static bool IsValidFileName(string fileName)
        {
            return !InvalidFileNameRegex.IsMatch(fileName);
        }

        public static Stream LoadResource(string name)
        {
            return typeof(IOUtility).Assembly.GetManifestResourceStream(name);
        }

        public static bool IsParent(string parent, string file)
        {
            if (string.IsNullOrEmpty(parent))
                return false;

            parent = parent.Replace('/', '\\');
            file = file.Replace('/', '\\');
            return file.StartsWith(parent);
        }

        private static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        public static IEnumerable<Tuple<string, string>> ParseProperties(Stream resource)
        {
            string line;
            using (var reader = new StreamReader(resource))
            {
                while ((line = reader.ReadLine()) != null)
                {
                    if (!line.StartsWith("#"))
                    {
                        var splits = line.Split('=');
                        if (splits.Length == 2)
                        {
                            var key = splits[0];
                            var value = splits[1];
                            yield return new Tuple<string, string>(key, value);
                        }
                    }
                }
            }
        }
    }
}
