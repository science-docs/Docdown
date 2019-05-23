using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Docdown.Util
{
    public static class PandocUtility
    {
        public static bool Exists()
        {
            return ProgramUtility.DoesExecute("pandoc", "-v");
        }

        public static string Compile(string fileName, string workspace, string template, string csl, string ext)
        {
            var arguments = new List<string>();
            arguments.AddRange(new string[] { "-f", "markdown", "--resource-path", workspace, "-V", $"resources={workspace.Replace('\\', '/')}", "--wrap=none" });
            var pdfName = Path.ChangeExtension(fileName, ".pdf");

            if (template == null)
            {
                arguments.Add("-s");
            }
            else
            {
                arguments.Add("--template");
                arguments.Add(template.Substring(0, template.LastIndexOf(".")));
            }

            if (csl != null)
            {
                arguments.Add("--csl");
                arguments.Add(csl);
            }

            foreach (var bibFile in Directory.GetFiles(workspace, "*.bib", SearchOption.AllDirectories))
            {
                arguments.Add("--bibliography");
                arguments.Add(bibFile);
            }

            arguments.Add("-o");
            arguments.Add(pdfName);

            arguments.Add("-t");
            arguments.Add("latex");

            foreach (var file in Directory.GetFiles(workspace, ext, SearchOption.AllDirectories))
            {
                arguments.Add(file);
            }

            var argumentList = new StringBuilder();

            foreach (var value in arguments)
            {
                argumentList.Append($"\"{value}\" ");
            }

            var argsString = argumentList.ToString();

            var exitCode = ProgramUtility.Execute("pandoc", argsString);

            if (exitCode != 0)
            {
                throw new InvalidProgramException("Pandoc finished unexpectedly");
            }
            File.Move(pdfName, fileName);
            return fileName;
        }
    }
}
