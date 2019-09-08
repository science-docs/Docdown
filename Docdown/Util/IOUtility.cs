using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Docdown.Util
{
    public static class IOUtility
    {
        private static readonly Regex InvalidFileNameRegex = new Regex("[" + new string(Path.GetInvalidFileNameChars()) + "]", RegexOptions.Compiled);
        private static readonly MD5 md5 = MD5.Create();

        public static async Task WriteAllBytes(IFileInfo fileInfo, byte[] bytes)
        {
            await WriteAllBytes(fileInfo.FullName, fileInfo.FileSystem.File, bytes);
        }

        public static async Task WriteAllText(IFileInfo fileInfo, string text)
        {
            await WriteAllBytes(fileInfo, Encoding.UTF8.GetBytes(text));
        }

        public static async Task<byte[]> ReadAllBytes(IFileInfo fileInfo)
        {
            using (var stream = fileInfo.OpenRead())
            {
                var result = new byte[stream.Length];
                await stream.ReadAsync(result, 0, (int)stream.Length);
                return result;
            }
        }

        public static async Task<string> ReadAllText(IFileInfo fileInfo)
        {
            return Encoding.UTF8.GetString(await ReadAllBytes(fileInfo));
        }

        public static async Task WriteAllBytes(string fullName, IFile file, byte[] bytes)
        {
            using (var stream = file.Open(fullName, FileMode.Create))
            {
                await stream.WriteAsync(bytes, 0, bytes.Length);
            }
        }

        public static async Task WriteAllText(string fullName, IFile file, string text)
        {
            await WriteAllBytes(fullName, file, Encoding.UTF8.GetBytes(text));
        }

        public static async Task<byte[]> ReadAllBytes(string fullName, IFile file)
        {
            using (var stream = file.OpenRead(fullName))
            {
                var result = new byte[stream.Length];
                await stream.ReadAsync(result, 0, (int)stream.Length);
                return result;
            }
        }

        public static async Task<string> ReadAllText(string fullName, IFile file)
        {
            return Encoding.UTF8.GetString(await ReadAllBytes(fullName, file));
        }

        public static string GetTempFile(IDirectory directory, string name = null)
        {
            string temp = Path.GetTempPath();
            const string docdown = "Docdown";
            string randomFile = (name ?? Guid.NewGuid().ToString()) + ".tmp";
            string file = Path.Combine(temp, docdown, randomFile);
            string dir = Path.Combine(temp, docdown);
            directory.CreateDirectory(dir);
            return file;
        }

        public static string GetHashFile(IDirectory directory, string valueToHash)
        {
            var bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(valueToHash));
            return GetTempFile(directory, ByteArrayToString(bytes));
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

        public static IEnumerable<Tuple<string, string>> ParseProperties(IFile file, string path)
        {
            using (var fs = file.OpenRead(path))
            {
                return ParseProperties(fs);
            }
        }

        public static void WriteProperties(IFile file, string path, IEnumerable<Tuple<string, string>> properties)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var prop in properties)
            {
                sb.Append(prop.Item1);
                sb.Append('=');
                sb.AppendLine(prop.Item2);
            }
            file.WriteAllText(path, sb.ToString());
        }
    }
}
