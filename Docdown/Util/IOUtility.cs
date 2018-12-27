using Docdown.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Docdown.Util
{
    public static class IOUtility
    {
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
    }
}
