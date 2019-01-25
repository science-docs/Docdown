using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Docdown.Util
{
    public static class IOUtility
    {
        private static readonly Regex InvalidFileNameRegex = new Regex("[" + new string(Path.GetInvalidFileNameChars()) + "]", RegexOptions.Compiled);

        public static string GetTempFile()
        {
            string temp = Path.GetTempPath();
            const string docdown = "Docdown";
            string randomFile = Guid.NewGuid().ToString() + ".tmp";
            string file = Path.Combine(temp, docdown, randomFile);
            string dir = Path.Combine(temp, docdown);
            Directory.CreateDirectory(dir);
            return file;
        }

        public static bool IsValidFileName(string fileName)
        {
            return !InvalidFileNameRegex.IsMatch(fileName);
        }

        public static Stream LoadResource(string name)
        {
            return typeof(IOUtility).Assembly.GetManifestResourceStream(name);
        }
    }
}
