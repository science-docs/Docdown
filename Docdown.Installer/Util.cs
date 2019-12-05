using System.IO;
using System.Threading.Tasks;

namespace Docdown.Installer
{
    public static class Util
    {
        public static async Task Extract(string resourceName, string path)
        {
            var asm = typeof(Util).Assembly;
            using (var rs = asm.GetManifestResourceStream(resourceName))
            {
                using (var fs = File.OpenWrite(path))
                {
                    await rs.CopyToAsync(fs);
                }
            }
        }
    }
}
