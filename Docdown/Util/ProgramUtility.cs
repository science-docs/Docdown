using System.Diagnostics;

namespace Docdown.Util
{
    public static class ProgramUtility
    {
        public static int Execute(string program, string args = null)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo(program, args)
                {
                    WindowStyle = ProcessWindowStyle.Hidden
                }
            };
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

        public static bool DoesExecute(string program, string args)
        {
            return Execute(program, args) == 0;
        }
    }
}
