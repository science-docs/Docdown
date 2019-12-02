using Docdown.Model;
using System.Text;

namespace Docdown.Text
{
    public abstract class Preprocessor : IPreprocessor
    {
        public byte[] Preprocess(IWorkspaceItem workspace, byte[] content)
        {
            var text = Encoding.UTF8.GetString(content);
            var processed = Preprocess(workspace, text);
            return Encoding.UTF8.GetBytes(processed);
        }

        public abstract string Preprocess(IWorkspaceItem workspace, string content);
    }
}
