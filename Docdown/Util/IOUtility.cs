using System;
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
    }
}
