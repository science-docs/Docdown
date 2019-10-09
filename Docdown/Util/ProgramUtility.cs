using System.Diagnostics;

namespace Docdown.Util
{
    public static class ProgramUtility
    {
        public static int Execute(string program, string args = null)
        {
            using (var process = new Process()
            {
                StartInfo = new ProcessStartInfo(program, args)
                {
                    WindowStyle = ProcessWindowStyle.Hidden
                }
            })
            {
                try
                {
                    process.Start();
                }
                catch
                {
                    return 1;
                }
                process.WaitForExit();
                return process.ExitCode;
            }
        }

        public static void ExecuteNonWaiting(string program, string args = null)
        {
#pragma warning disable IDE0067 // Objekte verwerfen, bevor Bereich verloren geht
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo(program, args)
                {
                    WindowStyle = ProcessWindowStyle.Hidden
                }
            };
#pragma warning restore IDE0067 // Objekte verwerfen, bevor Bereich verloren geht
            process.Exited += delegate { process.Dispose(); };
            try
            {
                process.Start();
            }
            catch
            {
            }
        }

        public static bool DoesExecute(string program, string args)
        {
            return Execute(program, args) == 0;
        }
    }
}
